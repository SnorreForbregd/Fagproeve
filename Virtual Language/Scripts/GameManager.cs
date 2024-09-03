using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Enumerator determining the languages the player can learn
    public enum _language{
        English
    };

    // Variable determining the current language to learn
    public _language Language;

    // Variable containing the text displayed to the user
    public string DisplayedItem;
    
    // Particle system used for confetti when the player wins
    public ParticleSystem ParticleSystem;

    // Transform that holds all the canvases for images
    public Transform CanvasHolder;

    // Internal score to count how many correct items the player has chosen in a row
    private int _score = 0;

    // The name of the current item to guess
    [SerializeField]
    private string _currentItem;


    // List of correctly guessed items
    private List<string> _guessedItems = new List<string>();

    // Lists of the items in the different languages
    private List<List<string>> _languageList = new List<List<string>>()
    {
        new List<string>() {"Apple", "Cucumber", "Fork", "Knife", "Chair", "Table", "Hand", "Spoon", "Tree", "Coffee"},
    };


    void Start()
    {   CanvasHolder.Find("ScoreCanvas/TextField/Text").GetComponent<TMP_Text>().SetText(_score.ToString());
        InitializeNewGuess();
    }

    // Check if the selected frame has the current item
    public void CheckData(GameObject ImageHolder)
    {
        Image image = ImageHolder.GetComponent<Image>();
        _guessedItems.Add(_currentItem);
        Feedback(image.sprite.name == _currentItem);
    }

    // Give feedback on whether the guess was correct or not
    public void Feedback(bool Success)
    {

        // If item was guessed correctly, increase score by 1, otherwise set to 0
        _score = (Success) ? _score+1 : 0;
        CanvasHolder.Find("ScoreCanvas/TextField/Text").GetComponent<TMP_Text>().SetText(_score.ToString());

        // If 5 items have been guessed correctly in a row, call the OnWin function
        if(_score >= 5) 
        {
            OnWin();
            return;
        }

        // If item was guessed incorrectly, reset list of items guessed this game
        if (!Success) _guessedItems = new List<string>();

        Debug.Log(_score);

        // Start new round of guessing
        InitializeNewGuess();
    }

    // Function responsible for celebration
    public void OnWin()
    {
        ParticleSystem.Play();
    }

    // Start a new round of guessing the item
    public void InitializeNewGuess()
    {
        // Get a list of all items not guessed correctly during this game
        // Both in base language (English), and the selected language
        List<string> newEngList = _languageList[(int)_language.English].FindAll(i => !_guessedItems.Contains(i));
        List<string> newLangList = _languageList[(int)Language].FindAll(i => !_guessedItems.Contains(_languageList[(int)_language.English][_languageList[(int)Language].IndexOf(i)]));
        
        // Randomly select what the correct item should be
        int _currentIndex = (int)UnityEngine.Random.Range(0, newEngList.Count-1);
        _currentItem = newEngList[_currentIndex];
        DisplayedItem = newLangList[_currentIndex];
        

        CanvasHolder.Find("Canvas/TextField/Text").GetComponent<TMP_Text>().SetText(DisplayedItem);

        // Get the sprite image corresponding to the current item
        Sprite CurrentSprite = Resources.Load<Sprite>(_currentItem);

        // Randomly select in which frame the correct item should be placed
        int rand = UnityEngine.Random.Range(1, 5);
        CanvasHolder.Find($"Canvas{rand}/Image").GetComponent<Image>().sprite = CurrentSprite;

        // List of items that have been added to the frames during this round
        List<string> UsedItems = new List<string>();
        
        // Add the current correct item to prevent duplicates
        UsedItems.Add(_currentItem);

        // Loop through the frames, and fill out every frame with a random item,
        // Except the frame with the current correct item
        for(int i = 1;i<=5;i++)
        {
            // If this is the frame we added the correct item to, skip this iteration of the loop
            if(i == rand) continue;

            // Get a random item that's not been placed in a frame yet
            string newItem = _languageList[(int)_language.English].FindAll(i => !UsedItems.Contains(i))[UnityEngine.Random.Range(0, _languageList[(int)_language.English].FindAll(i => !UsedItems.Contains(i)).Count-1)];
            // Add the item to the frame
            CanvasHolder.Find($"Canvas{i}/Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(newItem);
            // Add the item to the list of items that shouldn't be used anymore for this round
            UsedItems.Add(newItem);
        }

    }
}
