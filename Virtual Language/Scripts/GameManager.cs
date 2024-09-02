using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

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

    // Internal score to count how many correct object the player has chosen in a row
    private int _score = 0;

    // The name of the current object to guess
    [SerializeField]
    private string _currentItem;

    // List of correctly guessed items
    private List<string> _guessedItems = new List<string>();

    // Lists of the objects in the different languages
    private List<List<string>> _languageList = new List<List<string>>()
    {
        new List<string>() {"Apple", "Cucumber", "Fork", "Knife", "Chair", "Table", "Hand", "Spoon", "Tree", "Coffee"},
    };


    void Start()
    {
        int _currentIndex = (int)Math.Floor((float)UnityEngine.Random.Range(0, _languageList[(int)_language.English].Count-1));
        _currentItem = _languageList[(int)_language.English][_currentIndex];
        DisplayedItem = _languageList[(int)Language][_currentIndex];
        Debug.Log(_currentItem);
        Debug.Log(DisplayedItem);
    }

    public void CheckData(GameObject This)
    {
        Image image = This.GetComponent<Image>();
        if (image.sprite.name == _currentItem) {

        }
    }
}
