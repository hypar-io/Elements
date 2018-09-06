using System;
using System.Collections.Generic;

namespace AECSpaces
{
    public class AECFloor
    {

        public enum PlanType { B, E, F, H, L, T, U, X };

        AECSpaceGroup corridors; 
        AECSpace floor;
        // PlanType planType;
        AECSpaceGroup rooms;

        /// <summary>
        /// Contructor initializes classes.
        /// </summary>
        public AECFloor()
        {
            corridors = new AECSpaceGroup();
            floor = new AECSpace();
            // planType = PlanType.B;
            rooms = new AECSpaceGroup();
        }//constructor

        /// <summary>
        /// Returns the list of corridor spaces.
        /// </summary>
        public List<AECSpace> Corridors
        {
            get => corridors.Spaces;
        }//property

        /// <summary>
        /// Returns the floor envelope.
        /// </summary>
        public AECSpace Floor
        {
            get => floor;
            set => floor = value;
        }//property

        /// <summary>
        /// Returns the list of rooms.
        /// </summary>
        public List<AECSpace> Rooms
        {
            get => rooms.Spaces;
        }//property

    }//class
}//namespace
