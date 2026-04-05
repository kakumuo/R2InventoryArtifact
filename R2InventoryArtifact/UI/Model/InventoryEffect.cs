
using System;
using System.Collections.Generic;
using System.Linq;
using R2InventoryArtifact.Util.R2API;
using UnityEngine;

namespace R2InventoryArtifact.Model
{
    public enum InventoryEffectCode
    {
        None=0,
        BundleOfFireworks_BisonSteak_IgnitionTank=1
    }

    [Serializable] public class InventoryEffect
    {
        public string Name; 
        public string Description; 
    }

    //MAYBE: rename, idk if this is the right design pattern, but it feels correct
    public class InventoryEffectObserver
    {
        public Dictionary<InventoryEffectCode, int> EffectFreqs = new Dictionary<InventoryEffectCode, int>(); 
        private Dictionary<InventoryItem, Dictionary<InventoryEffectCode, int>> _itemEffectMap = new Dictionary<InventoryItem, Dictionary<InventoryEffectCode, int>>(); 
        private Dictionary<InventoryItem, HashSet<InventoryItem>> _adjList = new Dictionary<InventoryItem, HashSet<InventoryItem>>(); 

        public event Action<InventoryEffectCode, int> OnEffectAdd; 
        public event Action<InventoryEffectCode, int> OnEffectRemove; 

        public void AddAdjacency(InventoryItem itemA, InventoryItem itemB)
        {
            _adjList[itemA] = _adjList.GetValueOrDefault(itemA, new HashSet<InventoryItem>()); 
            _adjList[itemA].Add(itemB); 

            _adjList[itemB] = _adjList.GetValueOrDefault(itemB, new HashSet<InventoryItem>()); 
            _adjList[itemB].Add(itemA); 

            ValidateAdjacency(itemA); 
            ValidateAdjacency(itemB); 
        }

        public void RemoveAdjacency(InventoryItem itemA, InventoryItem itemB)
        {
            _adjList[itemA].Remove(itemB); 
            _adjList[itemB].Remove(itemA); 

            ValidateAdjacency(itemA); 
            ValidateAdjacency(itemB); 
        }

        private void ValidateAdjacency(InventoryItem parent)
        {
            HashSet<R2ItemCode> adjacentTo = _adjList.GetValueOrDefault(parent, new HashSet<InventoryItem>())
                .Select(i => i.ItemCode)
                .ToHashSet();
            Dictionary<InventoryEffectCode, int> effectFreqs = _itemEffectMap.GetValueOrDefault(parent, new Dictionary<InventoryEffectCode, int>())
                .Select(kvPair => new KeyValuePair<InventoryEffectCode, int>(kvPair.Key, 0))
                .ToDictionary(k => k.Key, v => v.Value);
            
            InventoryEffectCode resCode = InventoryService.GetInventoryEffectCode(parent.ItemCode, adjacentTo); 
            if(resCode != InventoryEffectCode.None)
            effectFreqs[resCode] = effectFreqs.GetValueOrDefault(resCode, 0) + 1; 


            List<(InventoryEffectCode, int)> addList = new (); 
            List<(InventoryEffectCode, int)> remList = new (); 
            foreach((InventoryEffectCode key, int newFreq) in effectFreqs)
            {
                int prevFreq = _itemEffectMap.GetValueOrDefault(parent, new Dictionary<InventoryEffectCode, int>()).GetValueOrDefault(key, 0); 

                if(newFreq == prevFreq) 
                    continue; 

                int updatedFreq = EffectFreqs.GetValueOrDefault(key, 0) - (prevFreq - newFreq); 
                if(newFreq > prevFreq)
                {
                    addList.Add((key, updatedFreq)); 
                } else if (prevFreq > newFreq)
                {
                    remList.Add((key, updatedFreq)); 
                }

                if(updatedFreq > 0) EffectFreqs[key] = updatedFreq;
                else EffectFreqs.Remove(key); 
            }

            addList.ForEach(set => OnEffectAdd?.Invoke(set.Item1, set.Item2));
            remList.ForEach(set => OnEffectRemove?.Invoke(set.Item1, set.Item2));

            _itemEffectMap[parent] = effectFreqs; 
        }
    }

}