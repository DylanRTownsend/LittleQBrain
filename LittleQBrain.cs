using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class LittleQBrain : MonoBehaviour
{
    //Class to use for our brain
    [System.Serializable]
    public class Open_QBrain_Class
    {
        //These can be changed to ints/float if needed
        public string State;
        public List<string> Actions;
    }

    [Header("Save File Path")]
    //The save directory for our new brain
    public string brainsSaveDirectory = "OpenBotBrain.brain";

    [Header("States and Actions")]
    //The list to update in inspector
    public List<Open_QBrain_Class> State_And_Actions_List = new List<Open_QBrain_Class>();

    //The qtable to contain our brain (Key = State, Value = Dictionary(Key = Action, Value = Q-Value)
    public ConcurrentDictionary<string, ConcurrentDictionary<string, float>> Open_QBrain_Container = new ConcurrentDictionary<string, ConcurrentDictionary<string, float>>();

    //For our Rewards
    [HideInInspector] public float rewardGranted;

    //States And Actions Capturing
    private string old_state;
    private string new_state;
    private string action_Choosen;

    //Variables for learning
    [Header("Q-Learning Values")]
    public float epsilon = 0.15f;
    [Tooltip("How quickly the bot adapts")] public float learningRate = 0.1f;
    [Tooltip("The importance of future rewards")] public float dicountFactor = 0.9f;
    
    //Our save manager class for saving and loading
    private SaveManager_LittleQ_Class Save_Manager = new SaveManager_LittleQ_Class();

    //Bool for checking if brain has been loaded/initialized
    [HideInInspector]public bool loaded;



    //Function for initializing brain
    public void InitializeQBRain()
    {
        //Set our path variable
        Save_Manager.pathDir = brainsSaveDirectory;
        //Set our path if we haven't already
        if (Save_Manager.path == null)
        {
            Save_Manager.path = Path.Combine(Application.persistentDataPath, Save_Manager.pathDir);
        }

        //If our states and actions list is empty (fine) if we have a brain ready to load up, else it will return an error
        if (State_And_Actions_List.Count <= 0 && !File.Exists(Save_Manager.path))
        {
            Debug.LogError("Please ensure their are States and Actions. Or a brain file path set.");
            return;
        }

        //If we do have an exisiting brain, load it up
        if (File.Exists(Save_Manager.path))
        {
            //Load up our exisiting brain (q-table)
            Save_Manager.LoadOQLBrain(ref Open_QBrain_Container);
        }
        else
        {
            //Initialize our brain for the first time, and save the newly intialized brain
            InitialzeANewOpenBrain();
        }


        //Set our bool to true
        loaded = true;


    }

    //Function for initalizing a new brain (IF YOU HAVE A BRAIN, THIS WILL WIPE IT CLEAN WITH A FRESH ONE).
    public void InitialzeANewOpenBrain()
    {
        //Ensure brain is clear
        Open_QBrain_Container.Clear();

        //A temp dictionary for our new values
        ConcurrentDictionary<string, float> new_action_qValues = new ConcurrentDictionary<string, float>();

        //Loop through our states and actions list
        foreach (Open_QBrain_Class s_AQ in State_And_Actions_List)
        {
            //Get our state
            string state = s_AQ.State;

            //Get all the actions associated, and set their qValue to 0.0f - adding to our temp dicitonary
            new_action_qValues.Clear();
            foreach (string action in s_AQ.Actions)
            {
                new_action_qValues.TryAdd(action, 0.0f);
            }

            //Add values to open brain 
            Open_QBrain_Container.TryAdd(state, new_action_qValues);


        }

        //Save our newly built brain
        Save_Manager.SaveOQLBrain(Open_QBrain_Container);

        //We don't need to save the experiences until we have some

    }


    //These are the only functions that need to be called to run the bot
    
    //Function called for making a choice
    public string MakeAChoice(string state)
    {
        //Set our reward variable back to 0

        //If we don't have a state yet
        //Not sure if we should do anything here


        //Get our curret state (we use the Get Current in case it is a new state)
        old_state = GetCurrentState(state);

        //Choose an action
        action_Choosen = ChooseAnAction(state);


        return action_Choosen;
    }
    //For Updating our reward, and our qTable
    public void UpdateReward(float rewarded, string currentState)
    {
        rewardGranted = 0;

        //Update our reward variable
        rewardGranted += rewarded;

        //Update our state variable
        new_state = GetCurrentState(currentState);

        //And Update our qTable variable
        UpdateQTable(old_state, new_state);


        //This causes a bottle neck because of the list sorting and other string functions if called too frequently
        /*//Save our updated qTable
        OpenQLearningBrainSave_Manager.SaveOQLBrain(Open_QBrain_Container);*/
    }
    //Function for saving the Q-table
    public void Save_QBrain(bool sorted = true)
    {
        Save_Manager.sortedOut = sorted;
        Save_Manager.SaveOQLBrain(Open_QBrain_Container);
    }

    //Action Functions

    //Choose an action uses one (of two) available methods to help balance the brains choice between exploration and exploitation
    public string ChooseAnAction(string state)
    {
        //Get a random value
        Random rand = new Random();
        float explore = (float)rand.NextDouble();
        //Debug.Log(explore);

        //Check if it less than our epsilon - epsilon is either set in inspector or updated dynamically in the Update Rewards function
        if (explore < epsilon)
        {
            //Get a random action
            int r = rand.Next(0, Open_QBrain_Container[state].Keys.Count);
            //Assign it
            string randomAction = Open_QBrain_Container[state].Keys.ElementAt(r);
            //Return it
            return randomAction;
        }
        else
        {
            //Set our action and best qvalue variables
            string bestChoice = string.Empty;
            float bestValue = float.MinValue;

            //Iterate through the brain
            for (int i = 0; i < Open_QBrain_Container[state].Keys.Count; i++)
            {
                //Set the qvalue
                float qValue = Open_QBrain_Container[state].Values.ElementAt(i);

                //If the value is greate than the best value
                if (qValue > bestValue)
                {
                    //Assing the value and the action to best choice
                    bestValue = qValue;
                    bestChoice = Open_QBrain_Container[state].Keys.ElementAt(i);
                }
            }

            //Return our action
            return bestChoice;
        }
    }


    //State Functions

    //Function for getting our current state - currently this is not very necessary,
    //since we can easily find out our state, but is good for adding in the case of meeting an unkown state
    public string GetCurrentState(string state)
    {
        //Create an empty variable
        string currentState = state;

        //If our current state does not match any state, we will add it to the brain using anothers states
        if (Open_QBrain_Container.ContainsKey(currentState))
        {
            return currentState;
        }
        else
        {
            //Create a new set of actions and qvalues (actions from state's actions) and save to our table
            ConcurrentDictionary<string, float> new_qValues_and_Actions = new ConcurrentDictionary<string, float>();

            //Random key int to use as an index to get a string key for the dictionary
            Random rand = new Random();
            int r = rand.Next(0, Open_QBrain_Container.Keys.Count);
            string rS = Open_QBrain_Container.ElementAt(r).Key;

            //Make a list from the actions, loop through, adding them to the new dictionary, with qValues of 0.0f
            List<string> actions = Open_QBrain_Container[rS].Keys.ToList();
            foreach (string action in actions)
            {
                new_qValues_and_Actions.TryAdd(action, 0.0f);
            }

            //Add our current (new) state and our current table, and save the table
            Open_QBrain_Container.TryAdd(currentState, new_qValues_and_Actions);
            Save_Manager.SaveOQLBrain(Open_QBrain_Container);

            return currentState;
        }

    }




    //Q Value/Table functions

    //Update QTable - No buffer replay experience
    public void UpdateQTable(string oldState, string newState)
    {
        // Get Q-values for the current state and action
        float currentQValue = GetQValue(oldState, action_Choosen);

        // Calculate the maximum Q-value for the next state (consider all possible actions)
        float maxQNext = GetMaxQValue(newState);

        // Update Q-value using the Bellman equation
        float newQValue = currentQValue + learningRate * (rewardGranted + dicountFactor * maxQNext - currentQValue);

        // Set the updated Q-value in the Q-table
        SetQValue(oldState, action_Choosen, newQValue);
    }
    //Get Q-Values
    float GetQValue(string state, string action)
    {
        ConcurrentDictionary<string, float> currentStateAndAction = Open_QBrain_Container[state];

        //if (currentStateAndAction[action] == null) { Debug.LogError("You are trying to save/access states with no actions. Follow this error message to where your script is calling the brain, and see what may be wrong there. (Also, your brain file likely has erronous data now, consider deleting it.)"); }

        return currentStateAndAction[action];
    }
    //Get Max Q-Value
    float GetMaxQValue(string newState)
    {
        //Find our next state in the dictionary, and collect it's ditionary of actions ang q-values
        ConcurrentDictionary<string, float> currentStateAndAction = Open_QBrain_Container[newState];

        float maxQ = float.MinValue; // Initialize with minimum value

        // Loop through all possible actions and check against/change the maxQ value

        foreach (KeyValuePair<string, float> kvp in currentStateAndAction)
        {
            maxQ = Mathf.Max(maxQ, currentStateAndAction[kvp.Key]);
        }


        return maxQ;
    }
    //Set Q-Value
    void SetQValue(string state, string action, float newQValue)
    {
        ConcurrentDictionary<string, float> currentStateAndAction = Open_QBrain_Container[state];

        // Update the Q-table entry with the new Q-value
        currentStateAndAction[action] = newQValue;
    }





    //Saving Class and functions
    public class SaveManager_LittleQ_Class
    {
        public string pathDir;
        public string path;
        public bool sortedOut = false;

        public void SaveOQLBrain(ConcurrentDictionary<string, ConcurrentDictionary<string, float>> bQtable)
        {
            //Set our path if we haven't already
            if (path == null)
            {
                path = Path.Combine(Application.persistentDataPath, pathDir);
            }


            //List for sorting our concurrent dictionaries output
            List<string> brainListStrings = new List<string>();
            //And and extra list for our header values
            //List<string> headerValues = new List<string>();

            //String we will add to the list and later reuse for the list to the saved file
            string toWrite = string.Empty;

            //Loop through our brain's states
            foreach (KeyValuePair<string, ConcurrentDictionary<string, float>> state in bQtable)
            {
                //Clear the string before writing it (since we are looping)
                toWrite = string.Empty;

                //Write our State to our string first
                toWrite = state.Key;
                //Add this to our header values
                //headerValues.Add(state.Key);

                //Create the dicitonary of actions and their q values
                ConcurrentDictionary<string, float> actions_qvalues = state.Value;
                //Loop through them and add them onto the string 
                foreach (KeyValuePair<string, float> actions in actions_qvalues)
                {
                    toWrite += "][" + actions.Key + "][" + actions.Value;
                }

                //Add the string to our list
                brainListStrings.Add(toWrite);

            }

            if (sortedOut)
            {
                //Sort both out lists
                brainListStrings.Sort();
            }

            //headerValues.Sort();

            try
            {
                //Using Stream Writer to write our simple brain
                using (StreamWriter writer = new StreamWriter(path))
                {

                    // Write header row from list of headers
                    /*string header = string.Empty;
                    foreach (string h in headerValues)
                    {
                        header += h + " ";
                    }
                    writer.WriteLine(header);*/

                    // Loop through the list using foreach
                    foreach (string state_actions_qvalue in brainListStrings)
                    {
                        writer.WriteLine(state_actions_qvalue);
                    }
                }
            }
            catch
            {
                Debug.Log("Not multithread, are we???");
            }



        }

        public void LoadOQLBrain(ref ConcurrentDictionary<string, ConcurrentDictionary<string, float>> bQtable)
        {
            //Set our path if we haven't already
            if (path == null)
            {
                path = Path.Combine(Application.persistentDataPath, pathDir);
            }

            //If our path does not exist, return out of the function
            if (!File.Exists(path)) return;

            //Ensure our table is clear
            bQtable.Clear();

            //Using streamer to read the saved file (above for reference)
            using (StreamReader reader = new StreamReader(path))
            {
                // Skip the header row 
                //reader.ReadLine();

                string line;
                string[] elements;

                while ((line = reader.ReadLine()) != null)
                {
                    elements = line.Split("][");

                    //First element for the dictionary, is our state
                    string state = elements[0];

                    //Second we need our list of actions and and key values
                    //We will do this by looping through the amount of elements (minus the state)
                    //And picking the odd (action comes after state, so is 1) actions, and even qvalues
                    //We can build our Concurrent Dicitionary
                    List<string> actions = new List<string>();
                    List<float> values = new List<float>();
                    for (int i = 1; i < elements.Length; i++)
                    {

                        if (i % 2 == 0)
                        {
                            //If even and it is a q-value
                            values.Add(float.Parse(elements[i]));
                        }
                        else
                        {
                            //Odd action
                            actions.Add(elements[i]);
                            //Debug.Log(state + " " + elements[i]);
                        }
                    }

                    //Our dicitonary of actions and qvalues 
                    ConcurrentDictionary<string, float> actions_qvalues = new ConcurrentDictionary<string, float>();
                    for (int i = 0; i < actions.Count; i++)
                    {
                        actions_qvalues.TryAdd(actions[i], values[i]);
                    }

                    //And now we can add our state_action-QValues to the dicitonary
                    bQtable.TryAdd(state, actions_qvalues);

                }
            }
        }

    }

}
