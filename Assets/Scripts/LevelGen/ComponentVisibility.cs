using UnityEngine;
using Catacumba.LevelGen;
using System;

namespace Catacumba {
    public class ComponentVisibility : MonoBehaviour
    {
        public bool IsMovable = true;

        [Header("Material configuration")]
        public string VisibilityProperty = "_Dissolve";

        private int _hashVisibilityProp = -1;


        private System.Collections.Generic.List<Renderer> _renderers;
        private System.Collections.Generic.List<Material> _materials;
        private Bounds _boundsCache;

        public Vector2Int _lastCellPosition;
        public Vector2Int cellPosition;

        private ComponentLevel componentLevel;

        // Start is called before the first frame update
        void Start()
        {
            this._hashVisibilityProp = Shader.PropertyToID(this.VisibilityProperty);
            this._materials = new System.Collections.Generic.List<Material>();
            this._renderers = new System.Collections.Generic.List<Renderer>(GetComponentsInChildren<Renderer>());
            foreach (var renderer in this._renderers) {
                this._materials.AddRange(renderer.materials);
            }

            this.componentLevel = FindObjectOfType<ComponentLevel>();
            this.componentLevel.Events.OnVisibilityMapChanged += Cb_OnVisibilityMapChanged;

            /*
            UpdateBounds();
            UpdateCellPosition();
            UpdateVisibility();
            */
        }

        void OnDestroy() 
        {
            this.componentLevel.Events.OnVisibilityMapChanged -= Cb_OnVisibilityMapChanged;
        }

        private void Cb_OnVisibilityMapChanged(VisibilityMap obj)
        {
            UpdateVisibility();
            //SetVisibility(this.componentLevel.VisibilityMap.GetVisibilityAt(_lastCellPosition));
        }

        void SetVisibility(float v) 
        {
            foreach (Material mat in this._materials) 
            {
                if (!mat.HasProperty(this._hashVisibilityProp))
                    continue;

                mat.SetFloat(this._hashVisibilityProp, v);
            }
        }

        void FixedUpdate()
        {
            if (!IsMovable)
                return;

            UpdateCellPosition();
            UpdateVisibility();
        }

        void UpdateBounds() {
            this._boundsCache = new Bounds();
            this._renderers.ForEach(r=>this._boundsCache.Encapsulate(r.bounds));
        }

        void UpdateVisibility() 
        {
            SetVisibility(this.componentLevel.VisibilityMap.GetVisibilityAt(cellPosition));
        }

        void UpdateCellPosition() {
            //Vector2Int cellPosition = this.componentLevel.WorldPositionToLevelPosition(GetCenter());
            //Vector2Int cellPosition = this.componentLevel.WorldPositionToLevelPosition(transform.position);
            //if (cellPosition != _lastCellPosition)
            //    SetVisibility(this.componentLevel.VisibilityMap.GetVisibilityAt(cellPosition));
            //_lastCellPosition = cellPosition;
        }

        Vector3 GetCenter() 
        {
            if (IsMovable) {
                UpdateBounds();
            }
            return this._boundsCache.center;
        }

    }
}