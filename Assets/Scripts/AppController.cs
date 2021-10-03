using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AppController : MonoBehaviour
{
    [SerializeField] private DishwasherGrid _dishwasherGrid;
    [SerializeField] private DishwasherOpener _dishwasherOpener;
    [SerializeField] private DishSelector _dishSelector;
    [SerializeField] private ResultPanel _resultPanel;

    [SerializeField] private int _freeSpacesToEndGame = 0;
    [SerializeField] private int _resultPanelShowingOffset = 0;

    [SerializeField] private string _winHeaderText;
    [SerializeField] private string _winScoreText;
    
    [SerializeField] private string _looseHeaderText;
    [SerializeField] private string _looseScoreText;
    
    private void Start()
    {
        _dishwasherOpener.Open();
        _dishwasherGrid.onFreeSpaceChanged += CheckEndGameCondition;
    }

    private void CheckEndGameCondition(int dishwasherFreeSpace)
    {
        if (dishwasherFreeSpace <= _freeSpacesToEndGame)
        {
            EndGame(_dishwasherGrid.IsFilledCorrectly());
        }
    }

    private void EndGame(bool isWin)
    {
        Destroy(_dishSelector);

        _dishwasherOpener.Close().OnComplete(() => { StartCoroutine(ShowResult(isWin)); });
    }

    private IEnumerator ShowResult(bool isWin)
    {
        yield return new WaitForSecondsRealtime(_resultPanelShowingOffset);

        string scoreText = isWin ? _winScoreText : _looseScoreText;
        string headerText = isWin ? _winHeaderText : _looseHeaderText;
        
        _resultPanel.SetTexts(scoreText, headerText);
        _resultPanel.Show();
    }
}
