using System;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public string saveTimeString;     // Lưu chuỗi ngày giờ (Ví dụ: "06/21/2026 10:21")
    public bool hasData = false;       // Đánh dấu ô này đã có dữ liệu lưu chưa

    // --- CHỈ SỐ PLAYER (STATS) ---
    public int playerCurrentHp;
    public int playerMaxHp;
    public int playerAttack;
    public int playerLevel;
    public int playerMana;
    public int playerMaxMana;
    public int playerShield ;
    public int maxShield;
    public int playerSp ;

    // --- TIẾN TRÌNH GAME ---
    public int currentStageIndex;      // Màn hiện tại đã hoàn thành

    // --- SKILL ĐÃ MỞ KHÓA ---
    public List<string> unlockedSkillNames = new List<string>();
}
