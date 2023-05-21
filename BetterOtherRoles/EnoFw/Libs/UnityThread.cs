// https://stackoverflow.com/questions/41330771/use-unity-api-from-another-thread-or-call-a-function-in-the-main-thread

#define ENABLE_UPDATE_FUNCTION_CALLBACK
#define ENABLE_LATEUPDATE_FUNCTION_CALLBACK
#define ENABLE_FIXEDUPDATE_FUNCTION_CALLBACK

using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Libs;

[RegisterInIl2Cpp]
public class UnityThread : MonoBehaviour
{
    //our (singleton) instance
    private static UnityThread _instance;


    ////////////////////////////////////////////////UPDATE IMPL////////////////////////////////////////////////////////
    //Holds actions received from another Thread. Will be coped to actionCopiedQueueUpdateFunc then executed from there
    private static readonly List<Action> ActionQueuesUpdateFunc = new();

    //holds Actions copied from actionQueuesUpdateFunc to be executed
    private readonly List<Action> _actionCopiedQueueUpdateFunc = new();

    // Used to know if whe have new Action function to execute. This prevents the use of the lock keyword every frame
    private static volatile bool _noActionQueueToExecuteUpdateFunc = true;


    ////////////////////////////////////////////////LATEUPDATE IMPL////////////////////////////////////////////////////////
    //Holds actions received from another Thread. Will be coped to actionCopiedQueueLateUpdateFunc then executed from there
    private static readonly List<Action> ActionQueuesLateUpdateFunc = new();

    //holds Actions copied from actionQueuesLateUpdateFunc to be executed
    private readonly List<Action> _actionCopiedQueueLateUpdateFunc = new();

    // Used to know if whe have new Action function to execute. This prevents the use of the lock keyword every frame
    private static volatile bool _noActionQueueToExecuteLateUpdateFunc = true;



    ////////////////////////////////////////////////FIXEDUPDATE IMPL////////////////////////////////////////////////////////
    //Holds actions received from another Thread. Will be coped to actionCopiedQueueFixedUpdateFunc then executed from there
    private static readonly List<Action> ActionQueuesFixedUpdateFunc = new List<Action>();

    //holds Actions copied from actionQueuesFixedUpdateFunc to be executed
    private readonly List<Action> _actionCopiedQueueFixedUpdateFunc = new List<Action>();

    // Used to know if whe have new Action function to execute. This prevents the use of the lock keyword every frame
    private static volatile bool _noActionQueueToExecuteFixedUpdateFunc = true;


    //Used to initialize UnityThread. Call once before any function here
    public static void InitUnityThread()
    {
        if (_instance != null)
        {
            return;
        }

        BetterOtherRolesPlugin.Logger.LogInfo($"Application.isPlaying: {Application.isPlaying}");
        if (Application.isPlaying)
        {
            _instance = BetterOtherRolesPlugin.Instance.AddComponent<UnityThread>();
        }
    }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    //////////////////////////////////////////////COROUTINE IMPL//////////////////////////////////////////////////////
#if (ENABLE_UPDATE_FUNCTION_CALLBACK)
    public static void ExecuteCoroutine(IEnumerator action)
    {
        if (_instance != null)
        {
            ExecuteInUpdate(() => _instance.StartCoroutine(action));
        }
    }

    ////////////////////////////////////////////UPDATE IMPL////////////////////////////////////////////////////
    public static void ExecuteInUpdate(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        lock (ActionQueuesUpdateFunc)
        {
            ActionQueuesUpdateFunc.Add(action);
            _noActionQueueToExecuteUpdateFunc = false;
        }
    }

    public void Update()
    {
        if (_noActionQueueToExecuteUpdateFunc)
        {
            return;
        }

        //Clear the old actions from the actionCopiedQueueUpdateFunc queue
        _actionCopiedQueueUpdateFunc.Clear();
        lock (ActionQueuesUpdateFunc)
        {
            //Copy actionQueuesUpdateFunc to the actionCopiedQueueUpdateFunc variable
            _actionCopiedQueueUpdateFunc.AddRange(ActionQueuesUpdateFunc);
            //Now clear the actionQueuesUpdateFunc since we've done copying it
            ActionQueuesUpdateFunc.Clear();
            _noActionQueueToExecuteUpdateFunc = true;
        }

        // Loop and execute the functions from the actionCopiedQueueUpdateFunc
        for (int i = 0; i < _actionCopiedQueueUpdateFunc.Count; i++)
        {
            _actionCopiedQueueUpdateFunc[i].Invoke();
        }
    }
#endif

    ////////////////////////////////////////////LATEUPDATE IMPL////////////////////////////////////////////////////
#if (ENABLE_LATEUPDATE_FUNCTION_CALLBACK)
    public static void executeInLateUpdate(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (ActionQueuesLateUpdateFunc)
        {
            ActionQueuesLateUpdateFunc.Add(action);
            _noActionQueueToExecuteLateUpdateFunc = false;
        }
    }


    public void LateUpdate()
    {
        if (_noActionQueueToExecuteLateUpdateFunc)
        {
            return;
        }

        //Clear the old actions from the actionCopiedQueueLateUpdateFunc queue
        _actionCopiedQueueLateUpdateFunc.Clear();
        lock (ActionQueuesLateUpdateFunc)
        {
            //Copy actionQueuesLateUpdateFunc to the actionCopiedQueueLateUpdateFunc variable
            _actionCopiedQueueLateUpdateFunc.AddRange(ActionQueuesLateUpdateFunc);
            //Now clear the actionQueuesLateUpdateFunc since we've done copying it
            ActionQueuesLateUpdateFunc.Clear();
            _noActionQueueToExecuteLateUpdateFunc = true;
        }

        // Loop and execute the functions from the actionCopiedQueueLateUpdateFunc
        for (int i = 0; i < _actionCopiedQueueLateUpdateFunc.Count; i++)
        {
            _actionCopiedQueueLateUpdateFunc[i].Invoke();
        }
    }
#endif

    ////////////////////////////////////////////FIXEDUPDATE IMPL//////////////////////////////////////////////////
#if (ENABLE_FIXEDUPDATE_FUNCTION_CALLBACK)
    public static void executeInFixedUpdate(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (ActionQueuesFixedUpdateFunc)
        {
            ActionQueuesFixedUpdateFunc.Add(action);
            _noActionQueueToExecuteFixedUpdateFunc = false;
        }
    }

    public void FixedUpdate()
    {
        if (_noActionQueueToExecuteFixedUpdateFunc)
        {
            return;
        }

        //Clear the old actions from the actionCopiedQueueFixedUpdateFunc queue
        _actionCopiedQueueFixedUpdateFunc.Clear();
        lock (ActionQueuesFixedUpdateFunc)
        {
            //Copy actionQueuesFixedUpdateFunc to the actionCopiedQueueFixedUpdateFunc variable
            _actionCopiedQueueFixedUpdateFunc.AddRange(ActionQueuesFixedUpdateFunc);
            //Now clear the actionQueuesFixedUpdateFunc since we've done copying it
            ActionQueuesFixedUpdateFunc.Clear();
            _noActionQueueToExecuteFixedUpdateFunc = true;
        }

        // Loop and execute the functions from the actionCopiedQueueFixedUpdateFunc
        foreach (var action in _actionCopiedQueueFixedUpdateFunc)
        {
            action.Invoke();
        }
    }
#endif

    public void OnDisable()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}