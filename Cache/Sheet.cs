/* Sheet.cs
 * Author: Taylor Howell
 */

using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using WarManager.Sharing;
using WarManager.Sharing.Security;
using StringUtility;

namespace WarManager.Backend
{
    [Serializable]
    /// <summary>
    /// Handles and stores the objects in cache
    /// </summary>
    /// <typeparam name="Tobj">the object type to store and manage (like a card?)</typeparam>
    [Notes.Author("Handles the cold data (layers, card locations, etc.) of each sheet (not the fluffy meta data)")]
    public class Sheet<Tobj> : IComparable<Sheet<Tobj>>, IEquatable<Sheet<Tobj>>, IWarManagerFile where Tobj : ICompareWarManagerPoint, IFileContentInfo
    {
        /// <summary>
        /// The ID of the sheet
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// The user defined name of the sheet
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The full file name (path) of the sheet
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Should the sheet be saved?
        /// </summary>
        /// <value></value>
        public bool Persistent { get; set; }

        /// <summary>
        /// The 2D array of partitions in the sheet
        /// </summary>
        public Dictionary<Layer, GridPartition<Tobj>[][]> Partitions { get; private set; }

        /// <summary>
        /// The list of assocated datasets
        /// </summary>
        /// <typeparam name="Dataset"></typeparam>
        /// <returns></returns>
        private List<string> _datasetIds = new List<string>();

        private List<string> _categories = new List<string>();

        /// <summary>
        /// The categories associated with the sheet
        /// </summary>
        /// <value></value>
        public string[] Categories
        {
            get
            {
                return _categories.ToArray();
            }

            set
            {
                _categories.Clear();
                _categories.AddRange(value);
            }
        }

        /// <summary>
        /// The layer currently being used in the sheet
        /// </summary>
        public Layer CurrentLayer { get; private set; } = new Layer("Default", 0, "#FFFF00", "d");

        /// <summary>
        /// The list of possible layers in the sheet
        /// </summary>
        private List<Layer> _layers = new List<Layer>();

        /// <summary>
        /// A dictionary that contains the string as an ID reference (key) to the Tobj (value)
        /// </summary>
        /// <typeparam name="string">the key id</typeparam>
        /// <typeparam name="Tobj">the object as value</typeparam>
        /// <returns></returns>
        public Dictionary<string, Tobj> ContentDict { get; private set; } = new Dictionary<string, Tobj>();

        /// <summary>
        /// The depth of the 2D partition array
        /// </summary>
        public Rect _totalSheetBounds = new Rect(new Point(0, 0), new Point(0, 0));

        /// <summary>
        /// The total start default when creating more capacity
        /// </summary>
        private int _arrDefaultStartSize = 5;

        public delegate void NewLayerSet(Layer layer);
        public static NewLayerSet OnSetLayer;


        /// <summary>
        /// The amount of cards contained in sheet
        /// </summary>
        public int CardCount { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Sheet(string id, string name, string fileName, bool persistant)
        {
            ID = id;
            Name = name;
            FileName = fileName;
            Persistent = persistant;
            Init();
        }

        /// <summary>
        /// Initialize the sheet
        /// </summary>
        private void Init()
        {
            CurrentLayer.CanDelete = false;
            AddNewLayer(CurrentLayer);
        }

        /// <summary>
        /// If checks to see if a layer exists in the sheet
        /// </summary>
        /// <param name="layerId">the id of the layer</param>
        /// <returns>returns true if the layer is not null, false if it is</returns>
        public bool LayerExists(string layerId)
        {
            if (GetLayer(layerId) != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the layer by layer ID
        /// </summary>
        /// <param name="layerId">the given id of the layer</param>
        /// <returns>returns a layer reference, null otherwise</returns>
        public Layer GetLayer(string layerId)
        {
            if (layerId == null || layerId == string.Empty)
                return null;

            for (int i = 0; i < _layers.Count; i++)
            {
                if (_layers[i].ID == layerId)
                {
                    return _layers[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the current layer to a new layer
        /// </summary>
        /// <param name="layer">the layer to set</param>
        public void SetCurrentLayer(string layerID)
        {
            if (layerID == null || layerID == string.Empty)
                return;

            if (layerID != CurrentLayer.ID)
            {
                Layer l = GetLayer(layerID);

                if (l != null)
                {
                    CurrentLayer = l;

                    if (OnSetLayer != null)
                    {
                        OnSetLayer(CurrentLayer);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new layer to the third dimension of the list and places the new layer into the list of layers
        /// </summary>
        public void AddNewLayer(Layer layer)
        {
            if (layer == null)
            {
                throw new NullReferenceException("Layer cannot be null");
            }

            int Capacity = _totalSheetBounds.Width;

            if (Capacity < _arrDefaultStartSize)
                Capacity = _arrDefaultStartSize;

            int depth = _totalSheetBounds.Height;

            if (depth < _arrDefaultStartSize)
                depth = _arrDefaultStartSize;

            GridPartition<Tobj>[][] newPartitionLayer = new GridPartition<Tobj>[Capacity][];

            for (int i = 0; i < Capacity; i++)
            {
                GridPartition<Tobj>[] dem = new GridPartition<Tobj>[depth];

                newPartitionLayer[i] = dem;
            }

            if (Partitions == null)
            {
                Partitions = new Dictionary<Layer, GridPartition<Tobj>[][]>();
            }

            Partitions.Add(layer, newPartitionLayer);

            _layers.Add(layer);
            _layers.Sort();

        }

        /// <summary>
        /// Adds an array of layers to the sheet
        /// </summary>
        /// <param name="layers">the given array of layers</param>
        public void AddLayers(Layer[] layers)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] != null)
                {
                    AddNewLayer(layers[i]);
                }
            }
        }

        /// <summary>
        /// Add a range of objects
        /// </summary>
        /// <param name="objs">the list of objects to add</param>
        /// <param name="layer">the layer to add objects to</param>
        public void AddObjRange(IList<Tobj> objs)
        {
            int totalx = 0;
            int totalY = 0;

            foreach (Tobj obj in objs)
            {
                if (obj.point.x > totalx)
                    totalx = obj.point.x;

                if (obj.point.y < totalY)
                    totalY = obj.point.y;
            }

            Point p = new Point(totalx, totalY);

            for (int j = 0; j < _layers.Count; j++)
            {
                for (int i = 0; i < Partitions[_layers[j]].Length; i++)
                {
                    GetPartition(p, _layers[j]);
                }
            }

            foreach (Tobj ob in objs)
            {
                if (ob != null && ob.Layer != null)
                {
                    AddObj(ob);

                }

                CardCount++;
            }
        }

        /// <summary>
        /// Clear the sheet of objs and re-initialize the sheet 
        /// </summary>
        public void Reset()
        {
            Partitions.Clear();
            Init();
        }

        /// <summary>
        /// Add an object to the sheet
        /// </summary>
        /// <param name="obj">the object to add</param>
        /// <param name="layer">the layer to reference</param>
        public bool AddObj(Tobj obj)
        {
            if (obj == null)
                throw new NullReferenceException("The object cannot be null");

            var layer = obj.Layer;

            if (layer == null)
                throw new NullReferenceException("The layer cannot be null");

            var partition = GetPartition(obj.point, layer);

            CardCount++;

            return partition.Add(obj);
        }

        /// <summary>
        /// Get the reference to the object
        /// </summary>
        /// <param name="p">the grid point </param>
        /// <param name="layer">the layer</param>
        /// <returns>returns the selected object (including null)</returns>
        public Tobj GetObj(Point p, Layer layer)
        {
            if (!PartitionExists(p, layer))
            {
                return default(Tobj);
            }

            var partition = GetPartition(p, layer);
            return partition.GetObj(p);
        }

        /// <summary>
        /// Compares to see if the object exists
        /// </summary>
        /// <param name="obj">the object to compare</param>
        /// <returns>returns true if the object exists, false if not</returns>
        public bool ContainsObj(Tobj obj)
        {
            var objs = GetAllObj();
            var theObj = objs.Find(x => (object)x == (object)obj);

            if (theObj != null)
                return true;

            return false;
        }

        /// <summary>
        /// Does the object exist?
        /// </summary>
        /// <param name="p">the point to look for</param>
        /// <param name="layer">the layer to look in</param>
        /// <returns>returns true if the object was found, false if not</returns>
        public bool ObjExists(Point p, Layer layer)
        {
            if (!PartitionExists(p, layer))
                return false;

            var partition = GetPartition(p, layer);
            return partition.Exist(p);
        }

        /// <summary>
        /// Remove an Object from the sheet
        /// </summary>
        /// <param name="p">the point location where the object is located</param>
        /// <param name="layer">the layer the object is in</param>
        public void Remove(Point p, Layer layer)
        {
            if (!PartitionExists(p, layer))
                return;

            var partition = GetPartition(p, layer);
            partition.Remove(p);

            CardCount--;
        }

        /// <summary>
        /// Remove the object from the sheet and return the reference
        /// </summary>
        /// <param name="p">the point location where the object exists</param>
        /// <param name="layer">the layer the object is in</param>
        /// <returns>returns the object occupying the location</returns>
        public Tobj RemoveObj(Point p, Layer layer)
        {
            if (!PartitionExists(p, layer))
                return default(Tobj);

            var obj = GetObj(p, layer);
            RemoveObj(p, layer);

            CardCount--;

            return obj;
        }

        /// <summary>
        /// Move an object from one point to the other
        /// </summary>
        /// <param name="start">where the object is currently located</param>
        /// <param name="end">where the object is now</param>
        /// <param name="startLayer">the layer where the object is</param>
        /// <param name="endLayer">the layer where the object will be moved to</param>
        /// <returns>return the object that was occupying the end location</returns>
        public Tobj MoveObj(Point start, Point end, Layer startLayer, Layer endLayer)
        {
            if (!PartitionExists(start, startLayer))
                throw new NotSupportedException("Partition does not exist " + start.ToString());

            var startPartition = GetPartition(start, startLayer);

            Tobj objToMove = startPartition.GetObj(start);
            startPartition.Remove(start);
            if (objToMove == null)
                throw new NotSupportedException("Cannot move a null card " + start.ToString());

            var endPartition = GetPartition(end, endLayer);
            Tobj occupyingObj = endPartition.GetObj(end);
            endPartition.Remove(end);

            objToMove.point = end;
            endPartition.Add(objToMove);

            return occupyingObj;
        }

        /// <summary>
        /// Replaces an existing object with a given one - returns the current object ref
        /// </summary>
        /// <param name="obj">the new object</param>
        /// <param name="layer">the layer</param>
        /// <returns>returns the object occupying the current space, returns null, if nothing exists at that location</returns>
        public Tobj Replace(Tobj obj, Layer layer)
        {
            if (!PartitionExists(obj.point, layer))
                return default(Tobj);

            var partition = GetPartition(obj.point, layer);
            var curObj = partition.GetObj(obj.point);

            partition.Add(obj);

            return curObj;
        }

        /// <summary>
        /// Returns the grid partitions from the sheet
        /// </summary>
        /// <returns>returns the grid positions if possible</returns>
        public List<GridPartition<Tobj>> GetAllGridPartitions()
        {
            List<GridPartition<Tobj>> partitions = new List<GridPartition<Tobj>>();

            for (int i = 0; i < _layers.Count; i++)
            {
                if (Partitions.ContainsKey(_layers[i]))
                {
                    for (int j = 0; j < Partitions[_layers[i]].Length; j++)
                    {
                        for (int k = 0; k < Partitions[_layers[i]][j].Length; k++)
                        {
                            if (Partitions[_layers[i]][j][k] != null && Partitions[_layers[i]][j][k].placeHolder == false)
                            {
                                var partition = Partitions[_layers[i]][j][k];
                                partitions.Add(partition);
                            }
                        }
                    }
                }
            }

            return partitions;
        }

        /// <summary>
        /// Get Grid Partitions
        /// </summary>
        /// <param name="layers">the array of layers</param>
        /// <returns>returns the list of grid partitions</returns>
        public List<GridPartition<Tobj>> GetGridPartitions(Layer[] layers)
        {

            if (layers == null || layers.Length == 0)
                return new List<GridPartition<Tobj>>();

            List<GridPartition<Tobj>> partitions = new List<GridPartition<Tobj>>();

            for (int i = 0; i < _layers.Count; i++)
            {
                if (layers.Contains(_layers[i]) && Partitions.ContainsKey(_layers[i]))
                {
                    for (int j = 0; j < Partitions[_layers[i]].Length; j++)
                    {
                        for (int k = 0; k < Partitions[_layers[i]][j].Length; k++)
                        {
                            if (Partitions[_layers[i]][j][k] != null && Partitions[_layers[i]][j][k].placeHolder == false)
                            {
                                var partition = Partitions[_layers[i]][j][k];
                                partitions.Add(partition);
                            }
                        }
                    }
                }
            }

            return partitions;
        }

        /// <summary>
        /// Get all the objects from the sheet
        /// </summary>
        /// <returns>returns a list of objects, if there are no objects, the list will be empty (not null)</returns>
        public List<Tobj> GetAllObj() // use linq for this if possible
        {
            List<Tobj> objs = new List<Tobj>();

            var parts = GetAllGridPartitions();

            foreach (var part in parts)
            {
                objs.AddRange(part.GetAllObjects());
            }

            return objs;
        }

        /// <summary>
        /// Get all obejcts in a sample layer
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        public List<Tobj> GetAllObj(IList<Layer> layers)
        {
            var parts = GetGridPartitions(layers.ToArray());

            List<Tobj> obj = new List<Tobj>();

            foreach(var x in parts)
            {
                obj.AddRange(x.GetAllObjects());
            }

            return obj;
        }

        /// <summary>
        /// Get the list of layers from the sheet
        /// </summary>
        /// <returns>returns a list of the layers</returns>
        public List<Layer> GetAllLayers()
        {
            return _layers;
        }

        /// <summary>
        /// Does the grid partition exist?
        /// </summary>
        /// <param name="p">the point to check</param>
        /// <param name="layer">the layer the object is in</param>
        /// <returns>return true if the partition exists, false if not</returns>
        public bool PartitionExists(Point p, Layer layer)
        {
            if (!Partitions.ContainsKey(layer))
            {
                return false;
            }

            Point arrP = GetPartitionCoordinate(p);

            int arrX = arrP.x;
            int arrY = arrP.y;

            if (Partitions[layer].Length <= arrX)
            {
                return false;
            }
            else if (Partitions[layer][arrX].Length <= arrY)
            {
                return false;
            }
            else if (Partitions[layer][arrX][arrY] == null || Partitions[layer][arrX][arrY].placeHolder)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the bounds of the sheet
        /// </summary>
        /// <returns>returns the bounds of the sheet</returns>
        public Rect GetGridSheetBounds()
        {
            return new Rect(_totalSheetBounds.TopLeftCorner, new Point(_totalSheetBounds.BottomRightCorner.x * 10, _totalSheetBounds.BottomRightCorner.y * 10));
        }

        /// <summary>
        /// Get the partition from a specified grid location
        /// </summary>
        /// <param name="p">the grid point</param>
        ///<param name="layer">the layer to edit</param>
        /// <returns>returns a grid partition</returns>
        private GridPartition<Tobj> GetPartition(Point p, Layer layer)
        {
            if (layer == null)
                throw new NullReferenceException("The layer cannot be null");

            if (!Partitions.ContainsKey(layer))
            {
                AddNewLayer(layer);
            }

            Point arrP = GetPartitionCoordinate(p);

            int arrX = arrP.x;
            int arrY = arrP.y;

            if (Partitions[layer].Length <= arrX)
            {
                ExtendPartitionArrayFirstDimension(arrX, layer);
            }

            if (Partitions[layer][arrX].Length <= arrY)
            {
                Partitions[layer][arrX] = ExtendPartitionArraySecondDimension(Partitions[layer][arrX], arrY, layer);
            }

            if (Partitions[layer][arrX][arrY] == null || Partitions[layer][arrX][arrY].placeHolder)
            {
                Partitions[layer][arrX][arrY] = new GridPartition<Tobj>(new Point(arrX * 10, -arrY * 10));
            }

            return Partitions[layer][arrX][arrY];
        }

        /// <summary>
        /// Creates more space in the partition array to add new grid partitions
        /// </summary>
        /// <param name="capacity">the new capacity of the array </param>
        /// <param name="layer">the layer to edit</param>
        private void ExtendPartitionArrayFirstDimension(int capacity, Layer layer)
        {
            int cap = capacity * 2;

            if (capacity < Partitions[layer].Length)
                throw new NotSupportedException("Capacity of the 2D partition array cannot shrink");

            GridPartition<Tobj>[][] newArray = new GridPartition<Tobj>[cap][]; // create a new array with the new capacity 

            for (int i = 0; i < Partitions[layer].Length; i++) // for every array in the first demension...
            {
                newArray[i] = ExtendPartitionArraySecondDimension(Partitions[layer][i], Partitions[layer][i].Length, layer); // extend the second demension array
            }

            for (int j = Partitions[layer].Length; j < newArray.Length; j++)
            {
                newArray[j] = new GridPartition<Tobj>[_arrDefaultStartSize];
            }

            Partitions.Remove(layer);
            Partitions.Add(layer, newArray);

            _totalSheetBounds.BottomRightCorner = new Point(capacity, _totalSheetBounds.BottomRightCorner.y); // update bounds
        }

        /// <summary>
        /// Creates more space in the partition array to add a new grid partitions
        /// </summary>
        /// <param name="xLocation">the x axis location of the jagged array</param>
        /// <param name="capacity">the capacity of the array</param>
        /// <param name="layer">the layer to edit</param>
        private GridPartition<Tobj>[] ExtendPartitionArraySecondDimension(GridPartition<Tobj>[] oldGrid, int capacity, Layer layer)
        {
            int cap = capacity * 2;

            //UnityEngine.Debug.Log(capacity + " " + oldGrid.Length);

            if (capacity < oldGrid.Length)
                throw new NotSupportedException("Capacity of the old grid cannot shrink");


            GridPartition<Tobj>[] temp = new GridPartition<Tobj>[cap];
            Array.Copy(oldGrid, temp, oldGrid.Length);

            if (_totalSheetBounds.Height > -capacity)
            {
                _totalSheetBounds.BottomRightCorner = new Point(_totalSheetBounds.BottomRightCorner.x, -capacity);
            }

            return temp;
        }


        /// <summary>
        /// Converts a grid coordinate into a partition array coordinate
        /// </summary>
        /// <param name="xCoordinate">the given grid coordinate to convert</param>
        /// <returns>returns an int partition coordiante</returns>
        public Point GetPartitionCoordinate(Point coordinate)
        {
            int xCoord = coordinate.x;
            int x = 0;

            if (xCoord >= 10)
            {
                var temp = xCoord % 10;
                x = (xCoord - temp) / 10;

            }

            int yCoord = -coordinate.y;

            int y = 0;

            if (yCoord >= 10)
            {
                var temp = yCoord % 10;
                y = (yCoord - temp) / 10;
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Get the dataset references from the sheet
        /// </summary>
        /// <returns>returns an array of datasets, if there are no datasets, an empty array will be returned </returns>
        public string[] GetDatasetIDs()
        {
            if (_datasetIds.Count > 0)
            {
                string[] copy = new string[_datasetIds.Count];
                var orig = _datasetIds.ToArray();

                Array.Copy(orig, copy, _datasetIds.Count);

                return copy;
            }
            else
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Does the sheet contain a certain data set?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsDataSet(string id)
        {
            foreach (var d in _datasetIds)
            {
                if (id == d)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Remove a dataset from the sheet
        /// </summary>
        /// <param name="id">the id of the dataset</param>
        /// <returns></returns>
        public bool RemoveDataset(string id)
        {
            if (id == null)
                throw new NullReferenceException("The id cannot be null");

            if (id == string.Empty)
                throw new NotSupportedException("the string cannot be empty");


            id = id.Trim();

            return _datasetIds.Remove(id);
        }

        /// <summary>
        /// Add a dataset to the sheet for reference 
        /// </summary>
        /// <param name="id">the id of the dataset</param>
        public void AddDataset(string id)
        {
            if (id == null)
                throw new NullReferenceException("The id cannot be null");

            if (id == string.Empty)
                throw new NotSupportedException("the string cannot be empty");

            id = id.Trim();

            _datasetIds.Add(id);
        }

        #region compare and equals


        public bool Equals(Sheet<Tobj> other)
        {
            return other.ID == ID;
        }

        public int CompareTo(Sheet<Tobj> other)
        {
            return ID.CompareTo(other.ID);
        }

        #endregion
    }
}
