using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Code found online: https://answers.unity.com/questions/1074814/is-it-possible-to-skew-or-shear-ui-elements-in-uni.html
/// </summary>
public class SkewedImage : Image
{
    public float skewX;
    public float skewY;

    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        var r = GetPixelAdjustedRect();
        var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
        Color32 color32 = color;
        vh.Clear();
        vh.AddVert( new Vector3( v.x - skewX, v.y - skewY ), color32, new Vector2( 0f, 0f ) );
        vh.AddVert( new Vector3( v.x + skewX, v.w - skewY ), color32, new Vector2( 0f, 1f ) );
        vh.AddVert( new Vector3( v.z + skewX, v.w + skewY ), color32, new Vector2( 1f, 1f ) );
        vh.AddVert( new Vector3( v.z - skewX, v.y + skewY ), color32, new Vector2( 1f, 0f ) );
        vh.AddTriangle( 0, 1, 2 );
        vh.AddTriangle( 2, 3, 0 );
    }

    /*
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);

        var height = rectTransform.rect.height;
        var width = rectTransform.rect.width;
        var xskew = height * Mathf.Tan(Mathf.Deg2Rad * skewX);
        var yskew = width * Mathf.Tan(Mathf.Deg2Rad * skewY);

        var y = rectTransform.rect.yMin;
        var x = rectTransform.rect.xMin;
        UIVertex v = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref v, i);
            v.position += new Vector3(Mathf.Lerp(0, xskew, (v.position.y - y) / height), Mathf.Lerp(0, yskew, (v.position.x - x) / width), 0);
            vh.SetUIVertex(v, i);
        }

    }
    */

    /*
    public Vector2 SkewVector;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        var r = GetPixelAdjustedRect();
        var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
        Color32 color32 = color;
        vh.Clear();
        vh.AddVert(new Vector3(v.x - SkewVector.x, v.y - SkewVector.y), color32, new Vector2(0f, 0f));
        vh.AddVert(new Vector3(v.x + SkewVector.x, v.w - SkewVector.y), color32, new Vector2(0f, 1f));
        vh.AddVert(new Vector3(v.z + SkewVector.x, v.w + SkewVector.y), color32, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(v.z - SkewVector.x, v.y + SkewVector.y), color32, new Vector2(1f, 0f));
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
    */
}
