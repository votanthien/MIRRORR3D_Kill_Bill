using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class GameGrid : MonoBehaviour
    {
        [System.Serializable]
        public struct PiecePrefab
        {
            public PieceType type;
            public GameObject prefab;
        };

        [System.Serializable]
        public struct PiecePosition
        {
            public PieceType type;
            public int x;
            public int y;
        };

        public int xDim;
        public int yDim;
        public float fillTime;

        public Level level;

        public PiecePrefab[] piecePrefabs;
        public GameObject backgroundPrefab;

        public PiecePosition[] initialPieces;

        private Dictionary<PieceType, GameObject> _piecePrefabDict;

        private GamePiece[,] _pieces;

        private bool _inverse;

        private GamePiece _pressedPiece;
        private GamePiece _enteredPiece;

        private bool _gameOver;

        public bool IsFilling { get; private set; }

        private void Update()
        {
            if (_gameOver || IsFilling) return;

            // Click chuột phải (Nút số 1)
            if (Input.GetMouseButtonDown(1))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

                if (hit.collider != null)
                {
                    GamePiece piece = hit.collider.GetComponent<GamePiece>();

                    // Nếu đúng là cục Rainbow
                    if (piece != null && piece.Type == PieceType.Rainbow)
                    {
                        if (level != null)
                        {
                            level.rpgEffectsEnabled = true; // Kích hoạt hệ thống RPG
                        }

                        Debug.Log("⚡ [KÍCH HOẠT] Chuột phải vào Tia Sét! Chỉ nổ damage và biến mất!");

                        // Chỉ xóa đúng vị trí của viên Rainbow này
                        ClearPiece(piece.X, piece.Y);

                        StartCoroutine(Fill());        // Cho kẹo mới rơi xuống điền chỗ trống
                        level.OnMove();               // Quái quật lại (nếu có logic turn)
                    }
                }
            }
        }
        private void Awake()
        {
            // populating dictionary with piece prefabs types
            _piecePrefabDict = new Dictionary<PieceType, GameObject>();
            for (int i = 0; i < piecePrefabs.Length; i++)
            {
                if (!_piecePrefabDict.ContainsKey(piecePrefabs[i].type))
                {
                    _piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
                }
            }

            // instantiate backgrounds
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    GameObject background = Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                    background.transform.parent = transform;
                }
            }

            // instantiating pieces
            _pieces = new GamePiece[xDim, yDim];

            for (int i = 0; i < initialPieces.Length; i++)
            {
                if (initialPieces[i].x >= 0 && initialPieces[i].y < xDim
                                            && initialPieces[i].y >=0 && initialPieces[i].y <yDim)
                {
                    SpawnNewPiece(initialPieces[i].x, initialPieces[i].y, initialPieces[i].type);
                }
            }

            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    if (_pieces[x, y] == null)
                    {
                        SpawnNewPiece(x, y, PieceType.Empty);
                    }                
                }
            }

            StartCoroutine(Fill());
        }
        // HÀM MỚI: Kiểm tra xem người chơi có vuốt gộp các cục nâng cấp (Lv2, Lv3) hay không
        private bool CheckSpecialMerge(GamePiece piece1, GamePiece piece2)
        {
            // Xác định cấp độ của 2 viên kẹo được vuốt
            bool p1_IsLv2 = (piece1.Type == PieceType.RowClear);
            bool p2_IsLv2 = (piece2.Type == PieceType.RowClear);
            bool p1_IsLv3 = (piece1.Type == PieceType.Lv3);
            bool p2_IsLv3 = (piece2.Type == PieceType.Lv3);

            // Cả hai viên phải có cấu phần màu (Cục Raw) và phải CÙNG MÀU với nhau mới gộp được
            if (piece1.IsColored() && piece2.IsColored() && piece1.ColorComponent.Color == piece2.ColorComponent.Color)
            {
                ColorType targetColor = piece1.ColorComponent.Color;

                // TRƯỜNG HỢP 1: Vuốt cục Lv2 vào cục Lv2 -> Tạo cục Lv3
                if (p1_IsLv2 && p2_IsLv2)
                {
                    int targetX = piece2.X; // Giữ lại vị trí của viên kẹo đích
                    int targetY = piece2.Y;

                    // Xóa trực tiếp GameObject của 2 viên Lv2 cũ (không kích hoạt hàm nổ thường)
                    DestroyPieceAt(piece1.X, piece1.Y);
                    DestroyPieceAt(piece2.X, piece2.Y);

                    // Sinh ra một viên Lv3 mới ngay tại vị trí đích và gán đúng màu cho nó
                    GamePiece newLv3 = SpawnNewPiece(targetX, targetY, PieceType.Lv3);
                    if (newLv3.IsColored())
                    {
                        newLv3.ColorComponent.Color = targetColor;
                    }

                    Debug.Log($"✨ [GỘP CẤP] Kết hợp 2 cục Lv2 thành 1 cục {targetColor} Lv3!");

                    // Cập nhật lại bàn cờ (cho kẹo trên cao rơi xuống lấp chỗ trống của viên piece1)
                    StartCoroutine(Fill());
                    return true; // Đã xử lý đặc biệt, dừng các logic match-3 thông thường
                }

                // TRƯỜNG HỢP 2: Vuốt cục Lv2 và cục Lv3 vào nhau -> Sát thương nhân 3 diện rộng
                if ((p1_IsLv2 && p2_IsLv3) || (p1_IsLv3 && p2_IsLv2))
                {
                    Debug.Log($"💥 [COMBO ĐẶC BIỆT] Kích nổ cục Lv2 + Lv3 cùng lúc! Hệ số x3!");

                    // Gọi sang Level để tính sát thương nhân 3 và dọn dẹp bàn cờ
                    level.ExecuteComboLv2Lv3(targetColor, piece1, piece2);
                    return true;
                }
            }

            return false; // Không phải combo gộp kẹo, tiếp tục xử lý match-3 như bình thường
        }

        // Hàm hỗ trợ xóa kẹo nhanh trên Grid mà không kích hoạt nổ dây chuyền sai logic
        public void DestroyPieceAt(int x, int y)
        {
            if (_pieces[x, y] != null)
            {
                Destroy(_pieces[x, y].gameObject);
                SpawnNewPiece(x, y, PieceType.Empty);
            }
        }
        public IEnumerator Fill()
        {        
            bool needsRefill = true;
            IsFilling = true;

            while (needsRefill)
            {
                yield return new WaitForSeconds(fillTime);
                while (FillStep())
                {
                    _inverse = !_inverse;
                    yield return new WaitForSeconds(fillTime);
                }

                needsRefill = ClearAllValidMatches();
            }

            IsFilling = false;
        }

        /// <summary>
        /// One pass through all grid cells, moving them down one grid, if possible.
        /// </summary>
        /// <returns> returns true if at least one piece is moved down</returns>
        private bool FillStep()
        {
            bool movedPiece = false;
            // y = 0 is at the top, we ignore the last row, since it can't be moved down.
            for (int y = yDim - 2; y >= 0; y--)
            {
                for (int loopX = 0; loopX < xDim; loopX++)
                {
                    int x = loopX;
                    if (_inverse) { x = xDim - 1 - loopX; }
                    GamePiece piece = _pieces[x, y];

                    if (!piece.IsMovable()) continue;
                
                    GamePiece pieceBelow = _pieces[x, y + 1];

                    if (pieceBelow.Type == PieceType.Empty)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.Move(x, y + 1, fillTime);
                        _pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.Empty);
                        movedPiece = true;
                    }
                    else
                    {
                        for (int diag = -1; diag <= 1; diag++)
                        {
                            if (diag == 0) continue;
                        
                            int diagX = x + diag;

                            if (_inverse)
                            {
                                diagX = x - diag;
                            }

                            if (diagX < 0 || diagX >= xDim) continue;
                        
                            GamePiece diagonalPiece = _pieces[diagX, y + 1];

                            if (diagonalPiece.Type != PieceType.Empty) continue;
                        
                            bool hasPieceAbove = true;

                            for (int aboveY = y; aboveY >= 0; aboveY--)
                            {
                                GamePiece pieceAbove = _pieces[diagX, aboveY];

                                if (pieceAbove.IsMovable())
                                {
                                    break;
                                }
                                else if (/*!pieceAbove.IsMovable() && */pieceAbove.Type != PieceType.Empty)
                                {
                                    hasPieceAbove = false;
                                    break;
                                }
                            }

                            if (hasPieceAbove) continue;
                        
                            Destroy(diagonalPiece.gameObject);
                            piece.MovableComponent.Move(diagX, y + 1, fillTime);
                            _pieces[diagX, y + 1] = piece;
                            SpawnNewPiece(x, y, PieceType.Empty);
                            movedPiece = true;
                            break;
                        }
                    }
                }
            }

            // the highest row (0) is a special case, we must fill it with new pieces if empty
            for (int x = 0; x < xDim; x++)
            {
                GamePiece pieceBelow = _pieces[x, 0];

                if (pieceBelow.Type != PieceType.Empty) continue;
            
                Destroy(pieceBelow.gameObject);
                GameObject newPiece = Instantiate(_piecePrefabDict[PieceType.Normal], GetWorldPosition(x, -1), Quaternion.identity, this.transform);

                _pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                _pieces[x, 0].Init(x, -1, this, PieceType.Normal);
                _pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                _pieces[x, 0].ColorComponent.SetColor((ColorType)Random.Range(0, _pieces[x, 0].ColorComponent.NumColors));
                movedPiece = true;
            }

            return movedPiece;
        }

        public Vector2 GetWorldPosition(int x, int y)
        {
            return new Vector2(
                transform.position.x - xDim / 2.0f + x,
                transform.position.y + yDim / 2.0f - y);
        }

        private GamePiece SpawnNewPiece(int x, int y, PieceType type)
        {
            GameObject newPiece = Instantiate(_piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity, this.transform);
            _pieces[x, y] = newPiece.GetComponent<GamePiece>();
            _pieces[x, y].Init(x, y, this, type);

            return _pieces[x, y];
        }

        private static bool IsAdjacent(GamePiece piece1, GamePiece piece2) =>
            (piece1.X == piece2.X && Mathf.Abs(piece1.Y - piece2.Y) == 1) ||
            (piece1.Y == piece2.Y && Mathf.Abs(piece1.X - piece2.X) == 1);

        private void SwapPieces(GamePiece piece1, GamePiece piece2)
        {
            if (_gameOver) { return; }

            if (level != null)
            {
                level.rpgEffectsEnabled = true;
            }

            if (!piece1.IsMovable() || !piece2.IsMovable()) return;

            // Vẫn giữ logic Gộp kẹo đặc biệt (Lv2 + Lv2 -> Lv3) từ bước trước của bạn
            if (CheckSpecialMerge(piece1, piece2))
            {
                _pressedPiece = null;
                _enteredPiece = null;
                return;
            }

            // Đổi chỗ nháp trong mảng dữ liệu để kiểm tra Match
            _pieces[piece1.X, piece1.Y] = piece2;
            _pieces[piece2.X, piece2.Y] = piece1;

            // CHỈ CHO PHÉP NỔ KHI CÓ MATCH 3 TRỞ LÊN (Giống hệt kẹo Normal)
            if (GetMatch(piece1, piece2.X, piece2.Y) != null ||
                GetMatch(piece2, piece1.X, piece1.Y) != null)
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                ClearAllValidMatches();

                _pressedPiece = null;
                _enteredPiece = null;

                StartCoroutine(Fill());
                level.OnMove();
            }
            else
            {
                // Nếu không tạo thành hàng 3 -> Đi qua rồi chạy về vị trí cũ
                _pieces[piece1.X, piece1.Y] = piece1;
                _pieces[piece2.X, piece2.Y] = piece2;

                StartCoroutine(AnimateInvalidSwap(piece1, piece2));

                _pressedPiece = null;
                _enteredPiece = null;
            }
        }

        // THÊM COROUTINE NÀY VÀO TRONG CLASS GameGrid
        private IEnumerator AnimateInvalidSwap(GamePiece piece1, GamePiece piece2)
        {
            // Lưu lại tọa độ thật của 2 viên kẹo (trước khi tráo đổi)
            int p1X = piece1.X;
            int p1Y = piece1.Y;
            int p2X = piece2.X;
            int p2Y = piece2.Y;

            // Bước 1: Di chuyển qua vị trí của nhau
            piece1.MovableComponent.Move(p2X, p2Y, fillTime);
            piece2.MovableComponent.Move(p1X, p1Y, fillTime);

            // Bước 2: Chờ cho animation "đi qua" chạy xong
            yield return new WaitForSeconds(fillTime);

            // Bước 3: Di chuyển lùi về vị trí ban đầu
            piece1.MovableComponent.Move(p1X, p1Y, fillTime);
            piece2.MovableComponent.Move(p2X, p2Y, fillTime);
        }

        public void PressPiece(GamePiece piece) => _pressedPiece = piece;

        public void EnterPiece(GamePiece piece) => _enteredPiece = piece;

        public void ReleasePiece()
        {
            if (IsAdjacent (_pressedPiece, _enteredPiece))
            {
                SwapPieces(_pressedPiece, _enteredPiece);
            }
        }

        private bool ClearAllValidMatches()
        {
            bool needsRefill = false;

            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    if (!_pieces[x, y].IsClearable()) continue;
                
                    List<GamePiece> match = GetMatch(_pieces[x, y], x, y);

                    if (match == null) continue;
                
                    PieceType specialPieceType = PieceType.Count;
                    GamePiece randomPiece = match[Random.Range(0, match.Count)];
                    int specialPieceX = randomPiece.X;
                    int specialPieceY = randomPiece.Y;

                    // Spawning special pieces
                    if (match.Count == 4)
                    {
                        if (_pressedPiece == null || _enteredPiece == null)
                        {
                            specialPieceType = (PieceType) Random.Range((int) PieceType.RowClear, (int) PieceType.ColumnClear);
                        }
                        else if (_pressedPiece.Y == _enteredPiece.Y)
                        {
                            specialPieceType = PieceType.RowClear;
                        }
                        else
                        {
                            specialPieceType = PieceType.ColumnClear;
                        }
                    } // Spawning a rainbow piece
                    else if (match.Count >= 5)
                    {
                        specialPieceType = PieceType.Rainbow;
                    }

                    foreach (var gamePiece in match)
                    {
                        if (!ClearPiece(gamePiece.X, gamePiece.Y)) continue;
                    
                        needsRefill = true;

                        if (gamePiece != _pressedPiece && gamePiece != _enteredPiece) continue;
                    
                        specialPieceX = gamePiece.X;
                        specialPieceY = gamePiece.Y;
                    }

                    // Setting their colors
                    if (specialPieceType == PieceType.Count) continue;
                
                    Destroy(_pieces[specialPieceX, specialPieceY]);
                    GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);

                    if ((specialPieceType == PieceType.RowClear || specialPieceType == PieceType.ColumnClear) 
                        && newPiece.IsColored() && match[0].IsColored())
                    {
                        newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
                    }
                    else if (specialPieceType == PieceType.Rainbow && newPiece.IsColored())
                    {
                        newPiece.ColorComponent.SetColor(ColorType.Any);
                    }
                }
            }

            return needsRefill;
        }

        private List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
        {
            if (!piece.IsColored()) return null;
            var color = piece.ColorComponent.Color;
            var horizontalPieces = new List<GamePiece>();
            var verticalPieces = new List<GamePiece>();
            var matchingPieces = new List<GamePiece>();

            // First check horizontal
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if (dir == 0)
                    { // Left
                        x = newX - xOffset;
                    }
                    else
                    { // right
                        x = newX + xOffset;                        
                    }

                    // out-of-bounds
                    if (x < 0 || x >= xDim) { break; }

                    // piece is the same color?
                    if (_pieces[x, newY].IsColored() && _pieces[x, newY].ColorComponent.Color == color)
                    {
                        horizontalPieces.Add(_pieces[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (horizontalPieces.Count >= 3)
            {
                matchingPieces.AddRange(horizontalPieces);
            }

            // Traverse vertically if we found a match (for L and T shape)
            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++ )
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int yOffset = 1; yOffset < yDim; yOffset++)                        
                        {
                            int y;
                            
                            if (dir == 0)
                            { // Up
                                y = newY - yOffset;
                            }
                            else
                            { // Down
                                y = newY + yOffset;
                            }

                            if (y < 0 || y >= yDim)
                            {
                                break;
                            }

                            if (_pieces[horizontalPieces[i].X, y].IsColored() && _pieces[horizontalPieces[i].X, y].ColorComponent.Color == color)
                            {
                                verticalPieces.Add(_pieces[horizontalPieces[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (verticalPieces.Count < 2)
                    {
                        verticalPieces.Clear();
                    }
                    else
                    {
                        matchingPieces.AddRange(verticalPieces);
                        break;
                    }
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }


            // Didn't find anything going horizontally first,
            // so now check vertically
            horizontalPieces.Clear();
            verticalPieces.Clear();
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < xDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { // Up
                        y = newY - yOffset;
                    }
                    else
                    { // Down
                        y = newY + yOffset;                        
                    }

                    // out-of-bounds
                    if (y < 0 || y >= yDim) { break; }

                    // piece is the same color?
                    if (_pieces[newX, y].IsColored() && _pieces[newX, y].ColorComponent.Color == color)
                    {
                        verticalPieces.Add(_pieces[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                matchingPieces.AddRange(verticalPieces);
            }

            // Traverse horizontally if we found a match (for L and T shape)
            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int xOffset = 1; xOffset < yDim; xOffset++)
                        {
                            int x;

                            if (dir == 0)
                            { // Left
                                x = newX - xOffset;
                            }
                            else
                            { // Right
                                x = newX + xOffset;
                            }

                            if (x < 0 || x >= xDim)
                            {
                                break;
                            }

                            if (_pieces[x, verticalPieces[i].Y].IsColored() && _pieces[x, verticalPieces[i].Y].ColorComponent.Color == color)
                            {
                                horizontalPieces.Add(_pieces[x, verticalPieces[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (horizontalPieces.Count < 2)
                    {
                        horizontalPieces.Clear();
                    }
                    else
                    {
                        matchingPieces.AddRange(horizontalPieces);
                        break;
                    }
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            return null;
        }

        private bool ClearPiece(int x, int y)
        {
            if (!_pieces[x, y].IsClearable() || _pieces[x, y].ClearableComponent.IsBeingCleared) return false;

            // --- THÊM MỚI: Báo cho hệ thống Level biết viên kẹo này nổ để tính sát thương/tia sét ---
            if (level != null)
            {
                level.OnPieceCleared(_pieces[x, y]);
            }
            // ---------------------------------------------------------------------------------------

            _pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.Empty);

            ClearObstacles(x, y);

            return true;
        }

        private void ClearObstacles(int x, int y)
        {
            for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
            {
                if (adjacentX == x || adjacentX < 0 || adjacentX >= xDim) continue;

                if (_pieces[adjacentX, y].Type != PieceType.Bubble || !_pieces[adjacentX, y].IsClearable()) continue;
            
                _pieces[adjacentX, y].ClearableComponent.Clear();
                SpawnNewPiece(adjacentX, y, PieceType.Empty);
            }

            for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
            {
                if (adjacentY == y || adjacentY < 0 || adjacentY >= yDim) continue;

                if (_pieces[x, adjacentY].Type != PieceType.Bubble || !_pieces[x, adjacentY].IsClearable()) continue;
            
                _pieces[x, adjacentY].ClearableComponent.Clear();
                SpawnNewPiece(x, adjacentY, PieceType.Empty);
            }
        }

        public void ClearRow(int row)
        {
            for (int x = 0; x < xDim; x++)
            {
                ClearPiece(x, row);
            }
        }

        public void ClearColumn(int column)
        {
            for (int y = 0; y < yDim; y++)
            {
                ClearPiece(column, y);
            }
        }

        public void ClearColor(ColorType color)
        {
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    if ((_pieces[x, y].IsColored() && _pieces[x, y].ColorComponent.Color == color)
                        || (color == ColorType.Any))
                    {
                        ClearPiece(x, y);
                    }
                }
            }
        }

        public void GameOver() => _gameOver = true;

        public List<GamePiece> GetPiecesOfType(PieceType type)
        {
            var piecesOfType = new List<GamePiece>();

            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    if (_pieces[x, y].Type == type)
                    {
                        piecesOfType.Add(_pieces[x, y]);
                    }
                }
            }

            return piecesOfType;
        }

    }
}
