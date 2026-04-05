using System;
using System.Collections.Generic;

namespace R2InventoryArtifact.Util.R2API
{
    [Serializable] public struct R2Item
    {
        public string Name; 
        public string Description;
        public R2ItemTier ItemTier; 
        public R2ItemCode ItemCode; 
    }

    [Serializable] public enum R2ItemTier
    {
        NONE, 
        T1,   T2,   T3,   
        BOSS, 
        LUNAR,
        VT1,  VT2,  VT3,  
        VBOSS,
    }

    [Serializable] public enum R2ItemCode
    {
        None=0,
        BundleOfFireworks=1, 
        BisonSteak, 
        IgnitionTank,
        RunaldsBand, 
        SingularityBand, 
        EmptyBottle,
    }
    
    public class R2ItemService
    {
        public static Dictionary<R2ItemCode, List<R2ItemCode>> Corrupt2Normal = new Dictionary<R2ItemCode, List<R2ItemCode>>()
        {
            {R2ItemCode.SingularityBand, new(){R2ItemCode.RunaldsBand}}
        };

        public static Dictionary<R2ItemCode, R2ItemCode> Normal2Corrupt = new Dictionary<R2ItemCode, R2ItemCode>()
        {
            {R2ItemCode.RunaldsBand, R2ItemCode.SingularityBand}
        }; 

        public static R2Item GetR2Item(R2ItemCode itemCode)
        {
            switch(itemCode)
            {
                case R2ItemCode.BundleOfFireworks: 
                    return new()
                    {
                        ItemCode=R2ItemCode.BundleOfFireworks, 
                        Name="Bundle of Fireworks", 
                        Description="", 
                        ItemTier=R2ItemTier.T1, 
                    }; 
                case R2ItemCode.IgnitionTank: 
                    return new()
                    {
                        ItemCode=R2ItemCode.IgnitionTank, 
                        Name="Ignition Tank", 
                        Description="", 
                        ItemTier=R2ItemTier.T2
                    }; 
                case R2ItemCode.BisonSteak: 
                    return new()
                    {
                        ItemCode=R2ItemCode.BisonSteak,
                        Name="Bison Steak", 
                        Description="", 
                        ItemTier=R2ItemTier.T1
                    };  
                case R2ItemCode.RunaldsBand: 
                    return new()
                    {
                        ItemCode=R2ItemCode.RunaldsBand,
                        Name="Runald's Band", 
                        Description="", 
                        ItemTier=R2ItemTier.T2
                    }; 
                case R2ItemCode.SingularityBand: 
                    return new()
                    {
                        ItemCode=R2ItemCode.SingularityBand,
                        Name="Singularity Band", 
                        Description="", 
                        ItemTier=R2ItemTier.VT2
                    }; 
                case R2ItemCode.EmptyBottle:
                    return new()
                    {
                        ItemCode=R2ItemCode.EmptyBottle,
                        Name="Empty Bottle", 
                        Description="", 
                        ItemTier=R2ItemTier.NONE
                    }; 
                default: return new R2Item(); 
            }
        }
    }

}