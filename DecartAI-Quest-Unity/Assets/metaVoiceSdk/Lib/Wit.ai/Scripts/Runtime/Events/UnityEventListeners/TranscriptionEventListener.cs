/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Events.UnityEventListeners
{
    public class TranscriptionEventListener : MonoBehaviour, ITranscriptionEvent
    {
        [SerializeField] public WitTranscriptionEvent onPartialTranscription = new
            WitTranscriptionEvent();
        [SerializeField] public WitTranscriptionEvent onFullTranscription = new
            WitTranscriptionEvent();

        public WitTranscriptionEvent OnPartialTranscription => onPartialTranscription;
        public WitTranscriptionEvent OnFullTranscription => onFullTranscription;

        private ITranscriptionEvent _events;

        private ITranscriptionEvent TranscriptionEvents
        {
            get
            {
                if (null == _events)
                {
                    var eventProvider = GetComponent<ITranscriptionEventProvider>();
                    if (null != eventProvider)
                    {
                        _events = eventProvider.TranscriptionEvents;
                    }
                }

                return _events;
            }
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable TranscriptionEventListener");
            var events = TranscriptionEvents;
            if (null != events)
            {
                events.OnPartialTranscription.AddListener(onPartialTranscription.Invoke);
                Debug.Log("OnEnable TranscriptionEventListener: OnPartialTranscription");
                events.OnFullTranscription.AddListener(onFullTranscription.Invoke);
                Debug.Log("OnEnable TranscriptionEventListener: OnFullTranscription");
            }
        }

        private void OnDisable()
        {
            var events = TranscriptionEvents;
            if (null != events)
            {
                events.OnPartialTranscription.RemoveListener(onPartialTranscription.Invoke);
                events.OnFullTranscription.RemoveListener(onFullTranscription.Invoke);
            }
        }
    }
}
