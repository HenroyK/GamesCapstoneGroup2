using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameContollerScript : MonoBehaviour
{
    public GameObject player;
    public GameObject playerSpawn;
    public int playerSpawnLayer;
    //private GameObject playerRef;

    public bool enableLifes = false;
    private LifesScript livesScript;


    public AudioManager audioManager;
    //list of all instantiated moving objects, ie. Buildings
    private List<GameObject> movingObjects = new List<GameObject>();

    //list of all instantiated moving objects to be saved at a checkpoint
    private List<GameObject> checkpointObjects = new List<GameObject>();

    //private int lastCheckpoint = 0;

    //List of Commands, such as spawning stuff, waiting or changing gamespeed.
    public List<Command> commandList = new List<Command>();
    [SerializeField]
    private int commandListIndex = 0;

    private float delay = 0;
    private float globalSpeed;
    [SerializeField]
    private float checkpointDelay;
    [SerializeField]
    private bool pauseTimer = false;



    //Below is stuff for later.
    //public List<GameObject> otherSpawnMovingObjects = new List<GameObject>();
    //public List<GameObject> FixedObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (audioManager = null)
        {
            audioManager = new AudioManager();
        }

        GameObject gameController = GameObject.FindWithTag("GameController");

        if (gameController != null)
        {
            livesScript = gameController.GetComponent<LifesScript>();
        }
        else
        {
            Debug.Log("Error. Couldn't find Game Controller");
        }

        if (!enableLifes)
        {
            livesScript.enabled = false;
			livesScript.livesUI.SetActive(false);
        }

        // sets the players Z axis spawn to the selected layer
        player.GetComponent<DepthBehaviour>().curDepth = playerSpawnLayer;
        float[] zPlayerSpawn = player.GetComponent<DepthBehaviour>().layerAxis;
        Vector3 playerSpawnPoint = new Vector3(
            playerSpawn.transform.position.x, 
            playerSpawn.transform.position.y, 
            zPlayerSpawn[playerSpawnLayer]);

        // Spawn player and set cameara to player character.
        // Also keeps a reference of the player in the scene
        player = Instantiate(player, playerSpawnPoint, Quaternion.identity);
        GameObject pla = GameObject.FindGameObjectWithTag("Player");
        pla.BroadcastMessage("UpdateSpeed", globalSpeed);
        this.GetComponent<CameraScript>().SetLookat(player);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //handle if the gamecontroller should wait before executing next command
        if(delay >= 0 && !pauseTimer)
        {
            delay -= Time.deltaTime;
        }

        while(delay <= 0 && commandList.Count() > commandListIndex)
        {
            //if not end of commands
            //get the next command from the list
            Command nextCommand = commandList.ElementAt(commandListIndex);
            commandListIndex++;
            switch (nextCommand.commandType)
            {
                case Command.CommandType.None:
                    Debug.LogWarning("Command with no type encountered.");
                    break;
                case Command.CommandType.Spawn:
                    if (nextCommand.spawnObject != null)
                    {
                        //Spawn Object
                        GameObject newObject = Instantiate(nextCommand.spawnObject, nextCommand.vector3, Quaternion.identity);
                        //Debug.Log("Spawning " + nextCommand.spawnObject + " at " + nextCommand.vector3 + ".");
                        //Set Object speed
                        newObject.BroadcastMessage("ChangeSpeed", globalSpeed);

                        //newObject.GetComponent<BlockMove>().ChangeSpeed(globalSpeed);
                        movingObjects.Add(newObject);
                    }
                    else
                    {
                        Debug.LogError("Invalid command (Spawn) -> need to fill Spawn Object");
                    }
                        
                    break;
                //case Command.CommandType.Wait:
                //    delay = nextCommand.time;
                //    Debug.Log("Waiting " + delay + " seconds.");
                //    break;
                //Change the global speed of all objects
                case Command.CommandType.ChangeSpeed:
                    globalSpeed = nextCommand.speed;
                    player.BroadcastMessage("UpdateSpeed", globalSpeed);
                    GameObject background = GameObject.FindWithTag("Background");
                    background.GetComponent<FloorMove>().BroadcastMessage("ChangeSpeed", globalSpeed);
                    foreach (GameObject a in movingObjects)
                    {
                        if (a != null)
                        {
                            a.BroadcastMessage("ChangeSpeed", globalSpeed);
                        }

                        
                        //a.GetComponent<BlockMove>().ChangeSpeed(globalSpeed);
                    }
                    
                    //Debug.Log("Global speed is now" + globalSpeed);
                    break;
                case Command.CommandType.Camera:
                    //make Camera look at worldpoint.
                    //NotImplemented
                    break;
                case Command.CommandType.Checkpoint:
                        //Set checkpoint to current command index for easy access. Create a copy of all objects in movingObjects and disable the copies.
                        //SetCheckpoint();
                        
                    break;
                case Command.CommandType.PlayAudio:
                    audioManager.AudioCommand(nextCommand.audioClip, nextCommand.audioDuration, nextCommand.audioVolume);
                    //NotImplemented
                    //Probably make a list of audio sources, place them into a list and use that to access them.
                    break;
                case Command.CommandType.SetLayer:
                    player.GetComponent<DepthBehaviour>().SetLayerState(nextCommand.layerNo, nextCommand.layerState);
                    break;
                case Command.CommandType.AwaitTrigger:
                    pauseTimer = true;
                    break;
                default:
                    Debug.LogError("Something has gone wrong -> no command type match or command not implemented.");
                    break;
            }
            delay = nextCommand.time;
            //Debug.Log("Waiting " + delay + " seconds.");
        }
    }

    // Disables / enables the movement controls for the player
    public void PlayerControls(bool state)
    {
        player.GetComponent<MovementScript>().enabled = state;
    }

    //public void SetCheckpoint()
    //{
    //    checkpointDelay = delay;
    //    //Handle saving checkpoint.
    //    lastCheckpoint = commandListIndex;
    //    checkpointObjects.Clear();

    //    foreach (GameObject obj in movingObjects)
    //    {
    //        GameObject newObj = Instantiate(obj);
    //        newObj.SetActive(false);
    //        checkpointObjects.Add(newObj);
    //    }
    //    this.GetComponent<ScoreScript>().SetCheckpoint();
    //}
    //public void LoadCheckpoint()
    //{
    //    //Handle loading checkpoint.
    //    clearObjects();
    //    foreach (GameObject obj in checkpointObjects)
    //    {
    //        GameObject newObj = Instantiate(obj);
    //        movingObjects.Add(newObj);
    //    }
    //    foreach (GameObject obj in movingObjects)
    //    {
    //        obj.SetActive(true);
    //        obj.BroadcastMessage("ChangeSpeed", globalSpeed);
    //    }
    //    //lateCheckpointUpdate = true;
    //    commandListIndex = lastCheckpoint;
    //    delay = checkpointDelay;
    //    player.transform.position = playerSpawn.transform.position;
    //    this.GetComponent<ScoreScript>().ResetCheckpoint();
    //}
    void clearObjects()
    {
        //Clear all moving objects.
        foreach (GameObject obj in movingObjects)
        {
            Destroy(obj);
        }
        movingObjects.Clear();
    }
    float GetCurrentSpeed()
    {
        return globalSpeed;
    }
    public void EndLevel()
    {
        //EndLevelCode
    }
    public void UnPauseTimer()
    {
        pauseTimer = false;
    }
}



//Code Archive

//list of scripted spawns. The Game Controller will spawn these in order, based on the instructions given elsewhere.
//public List<GameObject> scriptedSpawnMovingObjects = new List<GameObject>();