﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialValue : MonoBehaviour {

	public enum InitialValueType
	{
		None,
		Float,
		Int,
		Colour,
		Texture,
	};

	[Header("Apply (and set) all children's materials to first material found")]
	public bool MaterialOwnerInChildren = false;
	bool HasSetAllChildrensMaterial = false;

	public bool MaterialOwnerIsParent = false;

	//[ShowIf("NotUsingMaterialOwnerInChildren")]
	public MeshRenderer	materialOwner;
	//[ShowIf("NotUsingMaterialOwnerInChildren")]
	public Material	_material;


	public Material CachedMaterial
	{
		get
		{
			if (_material)
				return _material;

			Material m = null;

			if (m == null)
				m = GetMeshRendererMaterial ();

			if (m == null)
				m = GetRawImageMaterial ();

			if (m == null)
				m = GetTextMaterial ();

			//	if in editor, don't create a material instance, and don't cache
			if (!Application.isPlaying)
				return m;

			//	cache
			_material = m;
			return _material;
		}
	}

	bool NotUsingMaterialOwnerInChildren()
	{
		return !GlobalUniform && !MaterialOwnerInChildren;
	}

	Material GetMeshRendererMaterial()
	{
		MeshRenderer mr = materialOwner;
		if (mr == null) {
			mr = GetComponent<MeshRenderer> ();
		}
		if (mr == null)
			return null;
		
		return RealUseSharedMaterial ? mr.sharedMaterial : mr.material;
	}

	Material GetRawImageMaterial()
	{
		var ri = GetComponent<UnityEngine.UI.RawImage> ();
		if (ri == null)
			return null;

		return ri.material;
	}

	Material GetTextMaterial()
	{
		var t = GetComponent<UnityEngine.UI.Text> ();
		if (t == null)
			return null;

		return t.material;
	}

	//[ShowIfAttribute("MaterialIsNull")]
	public bool		UseSharedMaterial = true;
	public bool		RealUseSharedMaterial
	{
		get
		{
			return (UseSharedMaterial || !Application.isPlaying);
		}
	}


	public bool		GlobalUniform = false;
	public string	Uniform;

	[Header("If two-part value, this is the 2nd uniform. Dir in Ray. Inverse in matrix. Min&Max if Bounds")]
	public string	Uniform2;

	public InitialValueType	InitialiseValue = InitialValueType.None;

	[ShowIfAttribute("InitialValueIsFloat")]
	public float	InitialValue_Float = 0;
	[ShowIfAttribute("InitialValueIsInt")]
	public int		InitialValue_Int = 0;
	[ShowIfAttribute("InitialValueIsColour")]
	public Color	InitialValue_Colour = Color.white;
	[ShowIfAttribute("InitialValueIsTexture")]
	public Texture	InitialValue_Texture = null;

	public bool InitialValueIsFloat()	{	return InitialiseValue == InitialValueType.Float;	}
	public bool InitialValueIsInt()		{	return InitialiseValue == InitialValueType.Int;	}
	public bool InitialValueIsColour()	{	return InitialiseValue == InitialValueType.Colour;	}
	public bool InitialValueIsTexture()	{	return InitialiseValue == InitialValueType.Texture;	}

	bool	MaterialIsNull()
	{
		return _material == null;
	}

	void Start()
	{
		//	init
		switch (InitialiseValue) {
		case InitialValueType.Float:
			SetFloat (InitialValue_Float);
			break;

		case InitialValueType.Int:
			SetInt (InitialValue_Int);
			break;

		case InitialValueType.Colour:
			SetColor (InitialValue_Colour);
			break;

		case InitialValueType.Texture:
			SetTexture (InitialValue_Texture);
			break;

		}
	}

	void ForEachMaterial(System.Action<Material> Lambda)
	{
		//	todo; merge all this. Cache an array of materials, and set all children to the same material (option!)
		//	todo: make this an option, and cache a list of materials
		if (MaterialOwnerInChildren && !HasSetAllChildrensMaterial) {
		
			var mrs = GetComponentsInChildren<MeshRenderer> ();
			var ris = GetComponentsInChildren<UnityEngine.UI.RawImage> ();
			var ts = GetComponentsInChildren<UnityEngine.UI.Text> ();

			var cm = CachedMaterial;

			foreach (var mr in mrs) {
				if (mr.sharedMaterial == null)
					continue;
				if (cm == null)
					cm = RealUseSharedMaterial ? mr.sharedMaterial : mr.material;
				mr.sharedMaterial = cm;
			}
			foreach (var ri in ris) {
				if (ri.material == null)
					continue;
				if (cm == null)
					cm = ri.material;
				ri.material = cm;
			}
			foreach (var t in ts) {
				if (t.material == null)
					continue;
				if (cm == null)
					cm = t.material;
				t.material = cm;
			}

			HasSetAllChildrensMaterial = true;

			//	don't cache in editor
			if (!Application.isPlaying) {
				if (cm != null)
					Lambda.Invoke (cm);
				return;
			}
			_material = cm;

		}

		if (MaterialOwnerIsParent) {
		
			var Parent = transform.parent.gameObject;
			var mrs = Parent.GetComponents<MeshRenderer> ();
			var ris = Parent.GetComponents<UnityEngine.UI.RawImage> ();
			var ts = Parent.GetComponents<UnityEngine.UI.Text> ();

			var cm = CachedMaterial;

			foreach (var mr in mrs) {
				if (mr.sharedMaterial == null)
					continue;
				if (cm == null)
					cm = RealUseSharedMaterial ? mr.sharedMaterial : mr.material;
				mr.sharedMaterial = cm;
			}
			foreach (var ri in ris) {
				if (ri.material == null)
					continue;
				if (cm == null)
					cm = ri.material;
				ri.material = cm;
			}
			foreach (var t in ts) {
				if (t.material == null)
					continue;
				if (cm == null)
					cm = t.material;
				t.material = cm;
			}

			//	don't cache in editor
			if (!Application.isPlaying) {
				if (cm != null)
					Lambda.Invoke (cm);
				return;
			}
			_material = cm;

		}

		var mat = CachedMaterial;
		if (mat != null)
			Lambda.Invoke (mat);
	}


	public void SetMatrix(Matrix4x4 Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalMatrix (Uniform, Value);
			Shader.SetGlobalMatrix (Uniform2, Value.inverse);
		}
		else
		{
			ForEachMaterial ((m) => {
				m.SetMatrix (Uniform, Value);
				m.SetMatrix (Uniform2, Value.inverse);
			}
			);

		}
	}


	public void SetTexture(Texture Value)
	{
		if (GlobalUniform)
		{
			Shader.SetGlobalTexture (Uniform, Value);
		}
		else
		{
			ForEachMaterial ((m) => {
				m.SetTexture (Uniform, Value);
			});
		}
	}

	public void SetTexture2(Texture Value,Texture Valueb)
	{
		if (GlobalUniform)
		{
			Shader.SetGlobalTexture (Uniform, Value);
			Shader.SetGlobalTexture (Uniform2, Valueb);
		}
		else
		{
			ForEachMaterial ((m) => {
				m.SetTexture (Uniform, Value);
				m.SetTexture (Uniform2, Valueb);
			});
		}
	}

	public void SetRay2(Ray Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalVector (Uniform, Value.origin);
			Shader.SetGlobalVector (Uniform2, Value.direction);
		} 
		else
		{
			ForEachMaterial ((m) => {
				m.SetVector (Uniform, Value.origin);
				m.SetVector (Uniform2, Value.direction);
			});
		}
	}			
		
	public void SetVector4(Vector4 Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalVector (Uniform, Value);
		}
		else
		{
			ForEachMaterial ((m) => {
				m.SetVector (Uniform, Value);
			});
		}
	}


	public void SetVector3(Vector3 Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalVector (Uniform, Value);
		}
		else
		{
			ForEachMaterial ((m) => {
				m.SetVector (Uniform, Value);
			});
		}
	}


	public void SetFloat(float Value)
	{
        //Debug.Log(gameObject.name+" setting float ("+Uniform+") to "+Value.ToString());
		if (GlobalUniform) 
		{
			Shader.SetGlobalFloat (Uniform, Value);
		}
		else 
		{
			ForEachMaterial ((m) => {
				m.SetFloat (Uniform, Value);
			});
		}
	}

    public void SetColor1D(float Value)
    {
        if (GlobalUniform)
        {
            Shader.SetGlobalColor(Uniform, new Color(Value, Value, Value));
        }
        else
        {
            ForEachMaterial((m) => {
                m.SetColor(Uniform, new Color(Value, Value, Value));
            });
        }
    }


	public void SetInt(int Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalInt(Uniform, Value);
		}
		else			
		{
			ForEachMaterial ((m) => {
				m.SetInt (Uniform, Value);
			});
		}
	}


	public void SetColor(Color Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalColor (Uniform, Value);
		}
		else
		{
			ForEachMaterial ((m) => {
				m.SetColor (Uniform, Value);
			});
		}
	}


	public void SetBounds(Bounds bounds)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalVector (Uniform, bounds.min);
			Shader.SetGlobalVector (Uniform2, bounds.max);
		}
		else
		{
			ForEachMaterial ((m) => {
				m.SetVector (Uniform, bounds.min);
				m.SetVector (Uniform2, bounds.max);
			});
		}
	}

}
