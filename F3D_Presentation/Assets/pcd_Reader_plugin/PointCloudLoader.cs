using System.Collections;
using UnityEngine;

[System.Serializable]
public class Clouds
{
    public PointCloudData[] clouds;
}

[System.Serializable]
public class PointCloudData
{
    public PointPCD[] points;
    public ColorPoint[] colors;
}

[System.Serializable]
public class PointPCD
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class ColorPoint
{
    public float x;
    public float y;
    public float z;
}

public enum AnimationMode
{
    Loop,
    Boomerang
}

public class PointCloudLoader : MonoBehaviour
{
    [SerializeField] TextAsset jsonFile;
    Clouds clouds;
    int amountClouds;
    int iClouds = 0;
    private float frameTime = 1f / 30f;
    private float timer = 0f;

    int num;
    Mesh mesh;
    Vector3[] vertices;
    Color32[] colors;
    int[] indices;

    [SerializeField] AnimationMode animationMode;
    bool reverse = false;
    

    private void Start()
    {
        if (jsonFile == null)
        {
            Debug.LogError("jsonFile is not assigned.");
            return;
        }
        clouds = JsonUtility.FromJson<Clouds>(jsonFile.text);

        if (clouds == null)
        {
            Debug.LogError("Failed to deserialize JSON.");
            return;
        }
        InitMesh(clouds.clouds[0]);
        amountClouds = clouds.clouds.Length;
        Debug.Log(amountClouds);
        switch(animationMode)
        {
            case AnimationMode.Loop:
                StartCoroutine(PCDVideoLoop());
                break;
            case AnimationMode.Boomerang:
                StartCoroutine(PCDVideoBoomerang());
                break;
        }
        
    }
    private void InitMesh(PointCloudData pcdData)
    {
        
        num = pcdData.points.Length;

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        vertices = new Vector3[num];
        colors = new Color32[num];
        indices = new int[num];

        for (int i = 0; i < num; i++)
        {
            indices[i] = i;
        }

        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        gameObject.GetComponent<MeshFilter>().mesh = mesh;

        for (int i = 0; i < num; i++)
        {
            vertices[i] = new Vector3(pcdData.points[i].x, pcdData.points[i].y, pcdData.points[i].z);

            colors[i].b = (byte)(255 * pcdData.colors[i].z);
            colors[i].g = (byte)(255 * pcdData.colors[i].y);
            colors[i].r = (byte)(255 * pcdData.colors[i].x);
            colors[i].a = 255;
        }
        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.RecalculateBounds();
    }
    
    private IEnumerator PCDVideoLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => timer < frameTime);
            
            PointCloudData cloud = clouds.clouds[iClouds];
            p2vAndc2c(cloud);

            yield return new WaitForSeconds(frameTime);
        } 
    }

    private IEnumerator PCDVideoBoomerang()
    {
        while (true)
        {
            yield return new WaitUntil(() => timer < frameTime);
            PointCloudData cloud = null;
            if (!reverse)
            {
                cloud = clouds.clouds[iClouds];
            } else
            {
                cloud = clouds.clouds[amountClouds - iClouds - 1];
            }
            p2vAndc2c(cloud);
            yield return new WaitForSeconds(frameTime);
        }
    }

    private void p2vAndc2c(PointCloudData cloud)
    {
        int numPoints = Mathf.Min(cloud.points.Length, vertices.Length);
        for (int i = 0; i < numPoints; i++)
        {
            if (i >= cloud.points.Length) break;
            vertices[i] = new Vector3(cloud.points[i].x, cloud.points[i].y, cloud.points[i].z);
            colors[i].b = (byte)(255 * cloud.colors[i].z);
            colors[i].g = (byte)(255 * cloud.colors[i].y);
            colors[i].r = (byte)(255 * cloud.colors[i].x);
            colors[i].a = 255;
        }
        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.RecalculateBounds();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= frameTime)
        {
            timer -= frameTime;
            iClouds++;
            if (iClouds >= amountClouds)
            {
                reverse = !reverse;
                iClouds = 0;
            }
        }
    }
}