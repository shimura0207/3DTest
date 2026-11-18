/*
 *  @file   DeckSelectionManager
 *  @author oorui
 */

/// <summary>
/// 選択中のデッキ情報を管理するクラス
/// </summary>
public static class DeckSelectionManager {

    // 選択中のデッキID
    public static int SelectedDeckID { get; private set; } = -1;

    /// <summary>
    /// 選択中のデッキIDを設定する
    /// </summary>
    /// <param name="deckID">選択するデッキID</param>
    public static void SetSelectedDeckID(int deckID) {
        SelectedDeckID = deckID;
    }
}