using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class FoggyArea : MonoBehaviour
{
    private List<Vector4> collisionPolygon;
    private RenderTexture visitedAreas;
    private Material visitedAreasDrawer;
    private Material foggyArea;

    private void Start()
    {
        collisionPolygon = new List<Vector4>();
        foggyArea = GetComponent<MeshRenderer>().material;
        Texture2D unshadedTexture = (Texture2D)foggyArea.GetTexture("_MainTex");
        visitedAreas = new RenderTexture(unshadedTexture.width, unshadedTexture.height, 0, RenderTextureFormat.ARGBHalf);
        foggyArea.SetTexture("_VisitedAreas", visitedAreas);
        visitedAreasDrawer = new Material(Shader.Find("Unlit/VisitedAreasDrawer"));
        foggyArea.SetInt("_SourcesCount", FogManager.Instance.RevealersCount);
        foggyArea.SetInt("_VertexCount", FogManager.Instance.CollisionsCount);
        visitedAreasDrawer.SetInt("_SourcesCount", FogManager.Instance.RevealersCount);
        visitedAreasDrawer.SetInt("_VertexCount", FogManager.Instance.CollisionsCount);      
        visitedAreasDrawer.SetColor("_Color", Color.red);
        visitedAreasDrawer.SetTexture("_VisitedAreas", visitedAreas);
    }

    public void UpdateShaders(float viewingDiameter, (Vector4, bool)[] collisions, bool isMoving)
    {
        UpdateShaderPolygons(viewingDiameter, collisions);
        UpdateVisitedAreas(isMoving);
    }

    private void UpdateShaderPolygons(float viewingDiameter, (Vector4, bool)[] collisions)
    {
        collisionPolygon.AddRange(GetCollisionPolygon(viewingDiameter, collisions));
        if(collisionPolygon.Count == FogManager.Instance.RevealersCount * FogManager.Instance.CollisionsCount)
        {
            foggyArea.SetVectorArray("_Polygon", collisionPolygon);
            visitedAreasDrawer.SetVectorArray("_Polygon", collisionPolygon);
            collisionPolygon.Clear();
        }
    }

    private Vector4[] GetCollisionPolygon(float viewingDiameter, (Vector4, bool)[] collisions)
    {
        Vector4 firstCollision = collisions.FirstOrDefault(collision => collision.Item2).Item1;
        float roundedX = Mathf.Round(firstCollision.x);
        float roundedY = Mathf.Round(firstCollision.y);
        return collisions.Select(collision => collision.Item2 ?
            collision.Item1 : new Vector4(
            Mathf.Abs(firstCollision.x - collision.Item1.x) > viewingDiameter ? roundedX : collision.Item1.x,
            Mathf.Abs(firstCollision.y - collision.Item1.y) > viewingDiameter ? roundedY : collision.Item1.y)).ToArray();
    }

    private void UpdateVisitedAreas(bool isMoving)
    {
        RenderTexture temporaryTexture = RenderTexture.GetTemporary
            (visitedAreas.width, visitedAreas.height, 0, RenderTextureFormat.ARGBHalf);
        if(isMoving)
        {
            Graphics.Blit(visitedAreas, temporaryTexture);
            Graphics.Blit(temporaryTexture, visitedAreas, visitedAreasDrawer);
        }
        RenderTexture.ReleaseTemporary(temporaryTexture);
    }
}
