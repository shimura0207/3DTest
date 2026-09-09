/*
 *  @file   CommonModule
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;

/*
 * 汎用処理クラス
 */
public class CommonModule {
    /*
     * リストが空か判定
     */
    public static bool IsEmpty<T>(List<T> list) {
        return list == null || list.Count == 0;
    }

    public static bool IsEmpty<T>(T[] array) {
        return array == null || array.Length <= 0;
    }


    /*
     * リストに対して有効なインデックスか判定
     */
    public static bool IsEnableIndex<T>(List<T> list, int index) {
        if (IsEmpty(list)) return false;

        return index >= 0 && list.Count > index;
    }

    public static bool IsEnableIndex<T>(T[] array, int index) {
        if (IsEmpty(array)) return false;
        return index >= 0 && array.Length > index;
    }

    /*
     * リストを初期化する
     */
    public static void InitializeList<T>(ref List<T> list, int capacity = -1) {
        if (list == null) {
            if (capacity < 1) {
                list = new List<T>();
            }
            else {
                list = new List<T>(capacity);
            }
        }
        else {
            if (list.Capacity < capacity) list.Capacity = capacity;

            list.Clear();

        }
    }

    /*
     * リストを重複なしでマージ
     */
    public static void MeargeList<T>(ref List<T> main, List<T> sub) {
        if (IsEmpty(sub)) return;

        int meargeCount = sub.Count;
        if (main == null) main = new List<T>(meargeCount);

        for (int i = 0; i < meargeCount; i++) {
            // 重複した要素は追加しない
            if (main.Exists(mainElem => mainElem.Equals(sub[i]))) continue;
            main.Add(sub[i]);
        }

    }


    /*
     * 複数のタスクの終了を待つ
     */
    public static async UniTask WaitTask(List<UniTask> taskList) {
        // 終了したらタスクを知るとから除き、リストが空になるまで待つ
        while (true) {
            // 途中で要素が抜ける可能性があるので末尾から走査
            for (int i = taskList.Count - 1; i >= 0; i--) {
                // タスクが終了していたらリストから抜く
                if (!taskList[i].Status.IsCompleted()) continue;
                taskList.RemoveAt(i);
            }
            // リストが空ならループを抜ける
            if (IsEmpty(taskList)) break;

            await UniTask.DelayFrame(1);
        }
    }
}
