using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Hexagon.Scripts.Models
{
    public class Slot
    {
        public Vector3 position;
        public GameObject tileObject;

        private Slot()
        {
        }

        public Slot(Vector3 position)
        {
            this.position = position;
        }

        public GameObject CreateTile(GameObject prefab, Transform parent)
        {
            if (tileObject != null)
                DestroyTile();

            tileObject = GameObject.Instantiate(prefab, position + Vector3.up * 10f, Quaternion.identity, parent);
            return tileObject;
        }

        public GameObject ReplaceTile(GameObject prefab, Transform parent)
        {
            if (tileObject == null)
                return null;

            var color = tileObject.GetComponent<TileScript>().Color;
            DestroyTile();

            tileObject = GameObject.Instantiate(prefab, position, Quaternion.identity, parent);
            
            var tileScript = tileObject.GetComponent<TileScript>();
            tileScript.Color = color;
            return tileObject;
        }

        public void DestroyTile()
        {
            GameObject.Destroy(tileObject);
            tileObject = null;
        }

        public List<NodeScript> ConnectedNodes { get; } = new List<NodeScript>();
    }
}
