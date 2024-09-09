using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    // Values to add to to add new languages/Items in the code

    // Enumerator determining the languages the player can learn
    public enum _language{
        English,
        Norwegian,
    };

    // Lists of the items in the different languages
    private List<List<string>> _languageList = new List<List<string>>()
    {
        // English
        new List<string>() {"Apple", "Cucumber", "Fork", "Knife", "Chair", "Table", "Hand", "Spoon", "Tree", "Coffee"},
        // Norwegian
        new List<string>() {"Eple", "Agurk", "Gaffel", "Kniv", "Stol", "Bord", "Hand", "Skje", "Tre", "Kaffe"},
    };






    // Variable determining the current language to learn
    public _language Language;

    // Variable containing the text displayed to the user
    public string DisplayedItem;
    
    // Particle system used for confetti when the player wins
    public ParticleSystem ParticleSystem;

    // Transform that holds all the canvases for images
    public Transform CanvasHolder;
    
    // Dropdown menu
    public TMP_Dropdown Dropdown;

    // Just a field to simplify testing, displays the location of the correct item in the unity editor
    [SerializeField]
    private int _debug;


    // Internal score to count how many correct items the player has chosen in a row
    private int _score = 0;

    // The name of the current item to guess
    [SerializeField]
    private string _currentItem;


    // List of correctly guessed items
    private List<string> _guessedItems = new List<string>();

    // The game manager instance, which runs all the code
    public static GameManager Instance { get; private set; }





private void Awake() 
{ 
    // If there is an instance, and it's not this, delete this.
    
    if (Instance != null && Instance != this) 
    { 
        Debug.Log("Imposter destroyed");
        Destroy(this); 
    } 
    else 
    { 
        Debug.Log("Set to Self");
        Instance = this; 
    } 
}





    void Start()
    {   
        // Make this object be used throughout the entire game, so settings stay between scene transitions
        UnityEngine.Object.DontDestroyOnLoad(this);

        // Add an event to when a scene is loaded, so we can run stuff that would usually be in awake/start each time we transition between scenes
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Initialize the dropdown menu by filling it with all languages available
        foreach (string lang in (string[]) Enum.GetNames(typeof(_language)))
        {
            Debug.Log(lang);
            Dropdown.options.Add(item: new TMP_Dropdown.OptionData(image:Resources.Load<Sprite>($"Icons/Flags/Flag_{lang}")));
        }
        // Refresh the dropdown to display our changes
        Dropdown.RefreshShownValue();

    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    // Get the currently selected language
    public void GetDropdownOption()
    {
        string language = Dropdown.options[Dropdown.value].image.name.Substring(5);
        Enum.TryParse(language, out Language);
        Debug.Log(Language);
    }


    // Functions essentially as Awake, but is run for each scene despite this only being "woken up" once
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // The script that is run when the game is started
        if(scene.name == "GameScene") {
            // Due to an issue with the OnWinCanvas not being considered an UI element, It's initialized as active then disabled before the user loads in


            // Since this object doesn't exist in the game scene, it's required to get all references for this scene through the code
            // To simplify this, I created tags for the components needed, and applied that tag to it and only that object
            ParticleSystem = GameObject.FindGameObjectsWithTag("ParticleSystem")[0].GetComponent<ParticleSystem>();
            CanvasHolder = GameObject.FindGameObjectsWithTag("CanvasHolder")[0].transform;
            CanvasHolder.Find("OnWinCanvas").gameObject.SetActive(false);

            // Set the score canvas to display 0 (or whetever the score is, in case it is possible to gain score in the menu)
            CanvasHolder.Find("ScoreCanvas/TextField/Text").GetComponent<TMP_Text>().SetText(_score.ToString());
            
            // Set the menu button to call the ToMenu function when it is clicked after the game is over
            CanvasHolder.Find("OnWinCanvas/MenuButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ToMenu());
            
            // Make all the images tell the game manager they've been clicked when they are
            for(int i = 1;i<=5;i++)
            {
                Transform image = CanvasHolder.Find($"Canvas{i}/Image");
                image.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => CheckData(image.gameObject));
            }
            // Actually start the game
            InitializeNewGuess();
        }
        // Runs if the player is going from the game to the menu, currently only possible by winning the game
        if(scene.name == "Menu") {
            // Get the object references required in the Menu scene
            Transform MenuCanvas = GameObject.FindGameObjectsWithTag("MenuCanvas")[0].transform;
            Dropdown = MenuCanvas.Find("Dropdown").GetComponent<TMP_Dropdown>();
            
            // Set up the language dropdown again, since the scene has loaded again
            foreach (string lang in (string[]) Enum.GetNames(typeof(_language)))
            {
                Debug.Log(lang);
                Dropdown.options.Add(item: new TMP_Dropdown.OptionData(image:Resources.Load<Sprite>($"Icons/Flags/Flag_{lang}")));
            }
        // Set up the main menu buttons to still work
        // This isn't required in the actual awake, as it is set up through the editor
        // However, those settings stop working when the initial gamemanager from the menu-scene is deleted due to instancing
        MenuCanvas.Find("StartPanel").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => StartGame());
        MenuCanvas.Find("QuitPanel").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => QuitGame());

        // Set up the dropdown to actually change our language
        Dropdown.onValueChanged.AddListener((index) => GetDropdownOption());
        Dropdown.RefreshShownValue();
        }
    }

    // Check if the selected frame has the current item
    public void CheckData(GameObject ImageHolder)
    {
        UnityEngine.UI.Image image = ImageHolder.GetComponent<UnityEngine.UI.Image>();
        _guessedItems.Add(_currentItem);
        // Get feedback for the user based on their guess
        Feedback(image.sprite.name == _currentItem);
    }

    // Give feedback on whether the guess was correct or not
    public void Feedback(bool Success)
    {
        // If the game is won, no need to push it further
        if(_score == 5) return;
        // If item was guessed correctly, increase score by 1, otherwise set to 0
        _score = (Success) ? _score+1 : 0;
        CanvasHolder.Find("ScoreCanvas/TextField/Text").GetComponent<TMP_Text>().SetText(_score.ToString());

        // If 5 items have been guessed correctly in a row, call the OnWin function
        if(_score >= 5) 
        {
            _guessedItems = new List<string>();
            StartCoroutine(OnWin());
            return;
        }

        // If item was guessed incorrectly, reset list of items guessed this game
        if (!Success) _guessedItems = new List<string>();

        Debug.Log(_score);

        // Start new round of guessing
        InitializeNewGuess();
    }

    // Function responsible for celebration
    public IEnumerator OnWin()
    {
        // Confetti
        ParticleSystem.Play();
        // Wait for 2 seconds so the player can enjoy the confetti
        yield return new WaitForSeconds(2);
        // Display the "You won" UI
        CanvasHolder.Find("OnWinCanvas").gameObject.SetActive(true);

    }

    // Reset language choices and score, then return to the menu
    public void ToMenu() {
        _score = 0;
        Language = _language.English;
        SceneManager.LoadScene("Menu");
    }

    // Start a new round of guessing the item
    public void InitializeNewGuess()
    {
        // Get a list of all items not guessed correctly during this game
        // Both in base language (English), and the selected language
        List<string> newEngList = _languageList[(int)_language.English].FindAll(i => !_guessedItems.Contains(i));
        List<string> newLangList = _languageList[(int)Language].FindAll(i => !_guessedItems.Contains(_languageList[(int)_language.English][_languageList[(int)Language].IndexOf(i)]));
        
        // Randomly select what the correct item should be
        int _currentIndex = (int)UnityEngine.Random.Range(0, newEngList.Count);
        _currentItem = newEngList[_currentIndex];
        DisplayedItem = newLangList[_currentIndex];
        
        // Display what item the player should click
        CanvasHolder.Find("Canvas/TextField/Text").GetComponent<TMP_Text>().SetText(DisplayedItem);

        // Get the sprite image corresponding to the current item
        Sprite CurrentSprite = Resources.Load<Sprite>(_currentItem);

        // Randomly select in which frame the correct item should be placed
        int rand = UnityEngine.Random.Range(1, 6);
        _debug = rand;
        CanvasHolder.Find($"Canvas{rand}/Image").GetComponent<UnityEngine.UI.Image>().sprite = CurrentSprite;

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
            CanvasHolder.Find($"Canvas{i}/Image").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>(newItem);
            // Add the item to the list of items that shouldn't be used anymore for this round
            UsedItems.Add(newItem);
        }

    }
}
