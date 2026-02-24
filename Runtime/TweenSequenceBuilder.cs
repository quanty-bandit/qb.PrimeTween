using UnityEngine;
using PrimeTween;
using TriInspector;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System;
using qb.Events;

namespace qb.PrimeTween
{
    /// <summary>
    /// Component for building and controlling forward and backward tween animation sequences, supporting play, stop,
    /// toggle, and reset operations with event integration.
    /// </summary>
    [AddComponentMenu("qb/Animation/TweenSequenceBuilder")]
    [DeclareFoldoutGroup("#0", Title = "Global tween settingsContainer",Expanded =true)]
    [DeclareTabGroup("#1")]
    [DeclareFoldoutGroup("#2", Title = "Input channel commands")]
    [DeclareBoxGroup("#99",Title ="Control Panel")]
    public class TweenSequenceBuilder : MonoBehaviour
    {
        public enum EInSequenceMode { Chain, Group, Insert }

        [SerializeField, Group("#0")]
        bool isInfinitePlayLoop = false;


#if UNITY_EDITOR
        bool ShowCycleCount => !isInfinitePlayLoop;
        [ShowIf(nameof(ShowCycleCount))]
#endif
        [SerializeField,Min(1),Group("#0")]
        int cycles=1;

#if UNITY_EDITOR
        bool ShowCycleMode => cycles > 1 || isInfinitePlayLoop;
        [ShowIf(nameof(ShowCycleMode))]
#endif
        [SerializeField, Group("#0")]
        Sequence.SequenceCycleMode cycleMode = Sequence.SequenceCycleMode.Restart;
        [SerializeField, Group("#0")]
        Ease ease = Ease.Linear;
        [SerializeField, Group("#0")]
        bool useUnscaleTime = false;
        [SerializeField, Group("#0")]
        UpdateType updateType = default;
        
        [Title("Sequences")]
        [SerializeField]
        List<TweenSequenceEntry> forwardSequence = new List<TweenSequenceEntry>();


#if UNITY_EDITOR
        bool ShowGenerateBackSequenceButton => ShowBackwardsSequence && backwardSequence.Count == 0 && isInitialized;
        [Button,PropertyOrder(8),PropertySpace(spaceAfter:10),ShowIf(nameof(ShowGenerateBackSequenceButton))]
        void GenerateBackwardsSequence()
        {
            if (isInitialized)
                Reset();
            else
                ForceInitialization();
            for(int i= forwardSequence.Count-1; i>=0; i--)
            {
                backwardSequence.Add(forwardSequence[i].GetBackwardsGetter());
            }
        }
        bool ShowBackwardsSequence => forwardSequence.Count > 0;
        [ShowIf(nameof(ShowBackwardsSequence))]
#endif
        [SerializeField,PropertySpace(SpaceAfter =20)]
        List<TweenSequenceEntry> backwardSequence = new List<TweenSequenceEntry>();

        [Group("#1"), Tab("Event Channel")]
        [SerializeField]
        EventChannel onForwardStartChannel;
        [Group("#1"), Tab("Event Channel")]
        [SerializeField]
        EventChannel onForwardCompletedChannel;

        [Group("#1"),Tab("Unity Event")]
        public UnityEvent onForwardStart = new UnityEvent();
        [Group("#1"), Tab("Unity Event")]
        public UnityEvent onForwardCompleted = new UnityEvent();

        [Group("#1"), Tab("Event Channel")]
        [SerializeField]
        EventChannel onBackwardsStartChannel;
        [Group("#1"), Tab("Event Channel")]
        [SerializeField]
        EventChannel onBackwardsCompletedChannel;

        [Group("#1"), Tab("Unity Event")]
        public UnityEvent onBackwardsStart = new UnityEvent();
        [Group("#1"), Tab("Unity Event")]
        public UnityEvent onBackwardsCompleted = new UnityEvent();


        [Group("#1"), Tab("Event Channel")]
        [SerializeField]
        EventChannel onStopChannel;
        [Group("#1"), Tab("Unity Event")]
        public UnityEvent onStop = new UnityEvent();

        [Group("#2"), SerializeField]
        EventChannel playChannel;
        [Group("#2"), SerializeField]
        EventChannel playBackwardsChannel;
        [Group("#2"), SerializeField]
        EventChannel togglePlayChannel;
        [Group("#2"), SerializeField]
        EventChannel stopChannel;

        Sequence sequence;
        [NonSerialized]
        bool lastPlayIsForward;
        [NonSerialized]
        bool forwardCompleted,backwardCompleted;
        Sequence InitSequence(int cycleCount,Sequence.SequenceCycleMode cycleMode,Ease ease,bool useUnscaleTime,UpdateType updateType, List<TweenSequenceEntry> entries)
        {
            PrimeTweenConfig.warnEndValueEqualsCurrent = false;
            var sequence = Sequence.Create(cycleCount, cycleMode, ease, useUnscaleTime, updateType);
            float timeCursor = 0;
            int count = 0;
            foreach (var entry in entries)
            {
                var tween = entry.GetTween();
                  
                switch (entry.InSequenceMode)
                {
                    case EInSequenceMode.Chain:
                        if (entry.HasStartEvent)
                            sequence.ChainCallback(entry.InvokeStartEvent);
                        if (!entry.IsStartCallback)
                        {
                            sequence.Chain(tween);
                            if (count > 1)
                                timeCursor += tween.durationTotal;
                        }
                        break;

                    case EInSequenceMode.Group:
                        if (entry.HasStartEvent)
                            sequence.InsertCallback(timeCursor,entry.InvokeStartEvent);
                        if (!entry.IsStartCallback)
                            sequence.Group(tween); 
                        break;

                    case EInSequenceMode.Insert:
                        if (entry.HasStartEvent)
                            sequence.InsertCallback(entry.InsertTime, entry.InvokeStartEvent);
                        if (!entry.IsStartCallback)
                        {
                            sequence.Insert(entry.InsertTime, tween);
                            if (count > 1)
                                timeCursor = entry.InsertTime + tween.durationTotal;
                            else
                                timeCursor = entry.InsertTime;
                        }
                            break;
                }
                count++;
            }
            return sequence;
        }
        [NonSerialized]
        bool isInitialized=false;
        public bool IsInitialized=> isInitialized;

        public bool IsPlaying => sequence.isAlive;
        public enum EPlayingSequence { Forward,Backward,None}
        public EPlayingSequence CurrentPlayingSequence => (sequence.isAlive) ?(lastPlayIsForward)?EPlayingSequence.Forward:EPlayingSequence.Backward :EPlayingSequence.None;

#if UNITY_EDITOR
        bool ShowPlayButton=>forwardSequence.Count > 0 && isActiveAndEnabled && CurrentPlayingSequence!= EPlayingSequence.Forward && !forwardCompleted;
        [EnableIf(nameof(ShowPlayButton))]
#endif
        
        [Button(ButtonSizes.Large, "Play              ▶"), Tooltip("PLAY FORWARDS"), Group("#99")]
        public async Awaitable<bool> Play()
        {
            return await Play(isInfinitePlayLoop ? -1 : cycles);
        }
        public async Awaitable<bool> Play(int cycles)
        {
            if (cycles < -1 || cycles==0 || forwardSequence.Count == 0 || !isActiveAndEnabled) return false;

            if (sequence.isAlive)
            {
                if (!lastPlayIsForward)
                    sequence.Stop();
                else
                    return false;
            }
            else if (lastPlayIsForward && forwardCompleted)
                return false;

            Initialize();

            forwardCompleted = false;
            lastPlayIsForward = true;

            sequence = InitSequence(cycles,cycleMode,ease,useUnscaleTime,updateType,forwardSequence);

            onForwardStart.Invoke();
            onForwardStartChannel?.DispatchEvent();

            await sequence;
            forwardCompleted = true;
            backwardCompleted = false;

            onForwardCompleted.Invoke();
            onForwardCompletedChannel?.DispatchEvent();

            return true;
        }
#if UNITY_EDITOR
        bool ShowBackwardButton => backwardSequence.Count > 0 && isActiveAndEnabled && CurrentPlayingSequence != EPlayingSequence.Backward && !backwardCompleted;
        [EnableIf(nameof(ShowBackwardButton))]
#endif
        [Button(ButtonSizes.Large, "Backwards   ◀"), Tooltip("PLAY BACKWARDS"), Group("#99")]
        public async Awaitable<bool> PlayBackwards()
        {
            return await PlayBackwards(cycles);
        }

        public async Awaitable<bool> PlayBackwards(int cycles) 
        {
            if (cycles < -1 || cycles == 0 || backwardSequence.Count == 0 || !isActiveAndEnabled) return false;
            if (sequence.isAlive)
            {
                if (lastPlayIsForward)
                    sequence.Stop();
                else
                    return false;
            }
            else if (!lastPlayIsForward && backwardCompleted)
                return false;

            backwardCompleted = false;
            lastPlayIsForward = false;

            Initialize();

            sequence = InitSequence(cycles, cycleMode, ease, useUnscaleTime, updateType, backwardSequence);
            onBackwardsStart.Invoke();
            onBackwardsStartChannel?.DispatchEvent();

            await sequence;
            backwardCompleted = true;
            forwardCompleted = false;

            onBackwardsCompleted.Invoke();
            onBackwardsCompletedChannel?.DispatchEvent();

            return true;
        }
        

#if UNITY_EDITOR
        bool ShowToggleButton => backwardSequence.Count > 0 && forwardSequence.Count>0 && isActiveAndEnabled;
        [EnableIf(nameof(ShowToggleButton))]
#endif
        [Button(ButtonSizes.Large, "Toggle         ↔"),Tooltip("TOOGLE PLAY DIRECTION"), Group("#99")]
        public async Awaitable<bool> TogglePlay()
        {
            return await TogglePlay(cycles);
        }

        public async Awaitable<bool> TogglePlay(int cycles)
        {
            if (lastPlayIsForward)
                return await PlayBackwards(cycles);
            else
                return await Play(cycles);
        }

#if UNITY_EDITOR
        bool ShowStopButton => sequence.isAlive && isActiveAndEnabled;
        [EnableIf(nameof(ShowStopButton))]
#endif
        [Button(ButtonSizes.Large, "Stop             ■"), Tooltip("STOP"), Group("#99")]
        public void Stop()
        {
            if (sequence.isAlive)
            {
                sequence.Stop();
                onStop.Invoke();
                onStopChannel?.DispatchEvent();
            }
        }


#if UNITY_EDITOR
        bool ShowResetButton => isInitialized == true && isActiveAndEnabled;
        [EnableIf(nameof(ShowResetButton))]
#endif
        [Button(ButtonSizes.Medium, "Reset"), Group("#99"),PropertySpace(spaceBefore:10)]
        public void Reset()
        {
            if (!isInitialized) return;
            Stop();
            foreach (var entry in forwardSequence)
                entry.Reset();

            lastPlayIsForward = false;
            backwardCompleted = false;
            forwardCompleted = false;
        }


#if UNITY_EDITOR

        [Button(ButtonSizes.Medium, "Initialise Reset"), Group("#99")]
        void ForceInitialization()
        {
            Initialize(true);
        }
#endif
        void Initialize(bool force=false)
        {
            if (isInitialized && !force) return;
            foreach (var entry in forwardSequence)
                entry.Initialize(force);
            isInitialized = true;
        }

        public async void PlayAsync(int cycles)
        {
            await Play(cycles);
        }

        public async void PlayAsync()
        {
            await Play();
        }
        public async void PlayBackwardsAsync(int cycles)
        {
            await PlayBackwards(cycles);
        }
        public async void PlayBackwardsAsync()
        {
            await PlayBackwards();
        }
        public async void TogglePlayAsync()
        {
            await TogglePlay();
        }
        public async void TogglePlayAsync(int cycles)
        {
            await TogglePlay(cycles);
        }


        void PlayAsyncCommand(object sender) => PlayAsync();
        void PlayBackwardsAsyncCommand(object sender) => PlayBackwardsAsync();
        void TogglePlayAsyncCommand(object sender) => TogglePlayAsync();
        void StopCommand(object sender) => Stop();

        private void OnEnable()
        {
            if(playChannel)
                playChannel.Event += PlayAsyncCommand;
            if (playBackwardsChannel)
                playBackwardsChannel.Event += PlayBackwardsAsyncCommand;
            if(togglePlayChannel)
                togglePlayChannel.Event += TogglePlayAsyncCommand;
            if (stopChannel)
                stopChannel.Event += StopCommand;
        }

        private void OnDisable()
        {
            if (playChannel)
                playChannel.Event -= PlayAsyncCommand;
            if (playBackwardsChannel)
                playBackwardsChannel.Event -= PlayBackwardsAsyncCommand;
            if (togglePlayChannel)
                togglePlayChannel.Event -= TogglePlayAsyncCommand;
            if (stopChannel)
                stopChannel.Event -= StopCommand;
        }

#if UNITY_EDITOR
        bool ShowSwichSequencesButton => ShowBackwardsSequence && backwardSequence.Count > 0 ;
        [Button, PropertyOrder(9), PropertySpace(spaceAfter: 10), ShowIf(nameof(ShowSwichSequencesButton))]
        void SwitchSequences()
        {
            List<TweenSequenceEntry> tmpBack = new List<TweenSequenceEntry>();

            foreach(var entry in backwardSequence)
                tmpBack.Add(entry);

            backwardSequence.Clear();

            foreach(var entry in forwardSequence)
                backwardSequence.Add(entry);

            forwardSequence.Clear();
            foreach (var entry in tmpBack)
                forwardSequence.Add(entry);

        }
#endif
    }
}
