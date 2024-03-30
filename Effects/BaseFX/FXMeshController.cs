using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FX {
    public class FXMeshController : FXBase
    {

        public Mesh[] meshesSquare;
        public Mesh[] meshesRounded;
        public MeshFilter[] meshFilters;

        public enum MeshType
        {
            Square,
            Rounded
        }

        public FXParameter<MeshType> meshType = new FXParameter<MeshType>(MeshType.Square);


        protected override void Start()
        {
            meshesSquare = new Mesh[meshFilters.Length];
            for (int i = 0; i < meshFilters.Length; i++)
            {
                meshesSquare[i] = meshFilters[i].mesh;
            }

            meshType.OnValueChanged += OnMeshTypeChanged;
        }

        private void OnMeshTypeChanged(MeshType mesh)
        {
            Mesh[] selectedMeshes = null;

            switch (mesh)
            {
                case MeshType.Square:
                    selectedMeshes = meshesSquare;
                    break;
                case MeshType.Rounded:
                    selectedMeshes = meshesRounded;
                    break;
            }

            for (int i = 0; i < meshFilters.Length; i++)
            {
                meshFilters[i].mesh = selectedMeshes[i];
            }
        }

    }
}

