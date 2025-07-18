﻿using System.Collections.Generic;
using System.Linq;
using PeartreeGames.TriggerGraph.Triggers;
using UnityEngine;

namespace PeartreeGames.TriggerGraph
{
    [DisallowMultipleComponent]
    public class TriggerGraph : MonoBehaviour
    {
        [SerializeReference] public List<NodeData> nodes = new();
        public List<EdgeData> edges = new();
        
        [HideInInspector] public Vector3 viewPosition = Vector3.zero;
        [HideInInspector] public Vector3 viewScale = Vector3.one;
        
        private void Start()
        {
            ExecuteTriggers<LifecycleTrigger>(new TriggerContext(this, gameObject, nameof(LifecycleTrigger.Type.Start)));
        }

        private void OnEnable()
        {
            foreach (var node in nodes)
            {
                if (node is TriggerNode trigger) trigger.OnEnable(this);
            }
            ExecuteTriggers<LifecycleTrigger>(new TriggerContext(this, gameObject, nameof(LifecycleTrigger.Type.OnEnable)));
        }

        private void OnDisable()
        {
            foreach (var node in nodes)
            {
                if (node is TriggerNode trigger) trigger.OnDisable(this);
            }
            ExecuteTriggers<LifecycleTrigger>(new TriggerContext(this, gameObject, nameof(LifecycleTrigger.Type.OnDisable)));
        }

        private void ExecuteTriggers<T>(TriggerContext ctx) where T : TriggerNode
        {
            var triggers = GetTriggerNodes<T>();
            foreach (var trigger in triggers)
            {
                if (string.IsNullOrEmpty(ctx.Tag) || ctx.Tag.Equals(trigger.Tag))
                {
                    StartCoroutine(trigger.Execute(ctx, null));
                }
            }
        }

        public void InvokeEventTrigger(string triggerTag)
        {
            ExecuteTriggers<EventTrigger>(new TriggerContext(this, null, triggerTag));
        }

        public void InvokeEventTrigger(GameObject invoker, string triggerTag)
        {
            ExecuteTriggers<EventTrigger>(new TriggerContext(this, invoker, triggerTag));
        }
        
        private void OnTriggerEnter(Collider other)
        {
            ExecuteTriggers<CollisionTrigger>(new TriggerContext(this, other.gameObject,
                nameof(CollisionTrigger.Type.TriggerEnter)));
        }

        private void OnTriggerExit(Collider other)
        {
            ExecuteTriggers<CollisionTrigger>(new TriggerContext(this, other.gameObject,
                nameof(CollisionTrigger.Type.TriggerExit)));
        }

        private void OnCollisionEnter(Collision other)
        {
            ExecuteTriggers<CollisionTrigger>(new TriggerContext(this, other.gameObject,
                nameof(CollisionTrigger.Type.CollisionEnter)));
        }

        private void OnCollisionExit(Collision other)
        {
            ExecuteTriggers<CollisionTrigger>(new TriggerContext(this, other.gameObject,
                nameof(CollisionTrigger.Type.CollisionExit)));
        }

        private List<T> GetTriggerNodes<T>() where T : TriggerNode =>
            nodes.Where(n => n is T).Cast<T>().ToList();

        public List<NodeData> GetNextNodes(NodeData node, string outputPortName)
        {
            var edgeData = edges.FindAll(e =>
                e.OutputId == node.ID && e.outputPortName == outputPortName);
            return nodes.FindAll(n => edgeData.Exists(e => e.InputId == n.ID));
        }

        public List<NodeData> GetPrevNodes(NodeData node, string inputPortName)
        {
            var edgeData =
                edges.FindAll(e => e.InputId == node.ID && e.inputPortName == inputPortName);
            return nodes.FindAll(n => edgeData.Exists(e => e.OutputId == n.ID));
        }
    }
}