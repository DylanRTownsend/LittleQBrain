# LittleQBrain

### What is it?

Little Q Brain is a basic framework for using QLearning with a QTable in Unity, for helping Unity Devs learn the very basics of ML.

![Screenshot 2024-05-26 040716](https://github.com/DylanRTownsend/LittleQBrain/assets/43161779/193c6e97-60e6-4eca-b3b8-f59ab0ea2640)


- - - 

### How does it work?

Assuming the LittleQBrain.cs has been copied into Unity, the following steps will help demonstrate how:
1. Place the script onto an object to be used as a model (brain).
2. Within the Inspector the developer can set an example state and actions, the epsilon value, and the name of the model's persistent data (it's save file).
3. From here an agent and enviroment can be built by the developer. Within the agent's code, the following steps need to be taken:
    - Initalize the brain **InitializeQBRain()** (Only done once)
    - Make a choice **MakeAChoice()**
    - Act on the choice - a string is returned from MakeAChoice for the dev to act upon
    - Reward the model accordingly **UpdateReward()**
   
   The last three steps are looped, training the model.
4. Lastly the brain should be saved, allowing it to retain data between sessions.

When a choice is made - the dev passing in the current state of the agent when calling the function - the agents current state is saved as an **old state**, and the framework uses the epsilon-greedy exploration vs exploitation method to decide on what action to take. 

The action - string variable - is returned, allowing the agent to act (e.g. "Attack" is returned, developer makes the agent attack). 

The rewards earned are calculated by the developer, allowing them to freely train their model. Then the reward, and **current state** are passed into the UpdateReward() function, updating the QTable with the new values.

And by saving the new learned values, the models data becomes persistent.


- - -

### Where is everything in the code?

#### Initializing:
![Code1](https://github.com/DylanRTownsend/LittleQBrain/assets/43161779/15ef3fe3-1671-49b7-b59a-3356ea2459ae)

#### Model Controls:
![Code2](https://github.com/DylanRTownsend/LittleQBrain/assets/43161779/f56fa121-d301-43b6-af51-0e9eb37b108c)

#### Choice Decision Strategy:
![Code3](https://github.com/DylanRTownsend/LittleQBrain/assets/43161779/07c7ce8d-24ad-4c0e-ba5e-0f8cdc90ce2d)

#### State Check/Learn:
![Code4](https://github.com/DylanRTownsend/LittleQBrain/assets/43161779/784f73f8-a7e2-44b1-a82c-de44a696f908)

#### QTable/Value Functions:
![Code5](https://github.com/DylanRTownsend/LittleQBrain/assets/43161779/c41a8d7f-baf5-49d3-a971-e28f90957d5e)

#### Persistent Data:
![Code6](https://github.com/DylanRTownsend/LittleQBrain/assets/43161779/f8c25d35-93c3-47f5-b628-2fd85540c687)


- - -

### Additional Notes:

Little Q Brain uses StreamWriter and StreamReader for saving and loading the models data.

Models are based upon the files saved. On intilization, the path name (set in Inspector) is checked. If present, it loads that file's (model) data. Or creates a new one.

For learning new states, a simple learning method has been used: models states and actions are based on a singe state and actions base - i.e. all states have the same actions to choose from. This allows the model to take in a new state and apply the same action choices.

In the Inspector, alongside the epsilon value, is a learning rate and discount factor. These amounts are meta-variables to adjust the models learning.

Little Q Brain is a small Q Learning framework to help Unity Developers learn the basics of ML, therefore all the code is heavily commented.






