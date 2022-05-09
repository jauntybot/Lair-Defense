using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Scene dependent game manager, stores lair info and runs game states
// This class is baby compared to my pathfinding, but uses the same state machine format
public class LairManager : MonoBehaviour
{
    public enum LairState { Setup, Active, Results }
    public LairState currentState;

    public delegate void OnLairUpdate();
    public event OnLairUpdate CharacterUpdateCallback;
    public event OnLairUpdate ObjectUpdateCallback;

    [Header("References")] // Don't set these in the inspector! These are auto-populated
    public List<BaseCharacter> charactersInLair;
    public List<BaseObject> objectsInLair;
    public List<LairFeature> lairFeatures;
    // I don't hide them bc they're important for debugging

    #region Singleton
    public static LairManager instance;
    private void Awake()
    {
        if (LairManager.instance)
        {
            Debug.Log("Warning! More than one instance of LairManager found!");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    [Header("Setup Phase")]
    [SerializeField] GameObject setupTimerCanvas;
    [SerializeField] Text setupTimerText;
    [SerializeField] int setupDuration;
    int setupTime;

    [Header("Active Phase")]
    [SerializeField] LairEntrance entrance;
    [SerializeField] List<GameObject> invadingHeroes;
    [SerializeField] Transform heroSpawnPos;
    float heroSpawnDelay = 1;

    private void Start()
    {
        setupTime = setupDuration;
        ChangeState(LairState.Setup);
        StartCoroutine(RunStateMachine());
    }

    // Yield return yield return recursion! Not as much as my pathfinding, but simple statemachine for recurring calls
    IEnumerator RunStateMachine()
    {
        while (currentState != LairState.Results)
        {
            switch (currentState) {
                case LairState.Setup:
                    yield return StartCoroutine(SetupTimer());
                    break;
                case LairState.Active: // No recurring functions yet, but something like score keeping would go here
                    
                    yield return null;
                    break;
            }
        }
    }

    // Delayed loop to decrement timer and update UI, transitions state when done
    IEnumerator SetupTimer()
    {
        if (setupTime <= 0)
        {
            currentState = ChangeState(LairState.Active);
            yield return null;
        }
        else if (setupTime > 0)
        {
            setupTime --;
            if (setupTimerText)
                setupTimerText.text = setupTime.ToString();           
        }
        yield return new WaitForSeconds(1);

    }

    // public function for button
    public void SkipSetup()
    {
        setupTime = 0;
    }

    // When timer is up, start spawning heroes on a delay
    IEnumerator SpawnHeros() { 
        if (entrance)      
            entrance.ToggleEntranceState(true); // Open the entrance in it's LairFeature class
        
        foreach (GameObject hero in invadingHeroes)
        {
            Instantiate(hero, heroSpawnPos.position, Quaternion.identity);
            yield return new WaitForSeconds(heroSpawnDelay); // Delay the spawning
        }
    }

    //Transition states, trigger one-time functions
    LairState ChangeState(LairState targetState)
    {
        if (targetState == LairState.Setup)
        {
            setupTimerCanvas.SetActive(true);
        }
        if (targetState == LairState.Active)
        {
            setupTimerCanvas.SetActive(false);
            if (currentState == LairState.Setup)
                StartCoroutine(SpawnHeros());
        }

        return targetState;
    }

    // Clumsy functions that are executed through other references to this instance, in order to then call events....
    public void UpdateCharacterList()
    {
        CharacterUpdateCallback?.Invoke();
    }

    public void UpdateObjectList()
    {
        ObjectUpdateCallback?.Invoke();
    }
}
