using UnityEngine;
using UnityEngine.Rendering;
using HSVPicker;
public class PaintManager : Singleton<PaintManager>{

    public Shader texturePaint;
    public Shader extendIslands;
	public GameObject colourSelector;
	public Color selectedColour;

	int prepareUVID = Shader.PropertyToID("_PrepareUV");
    int positionID = Shader.PropertyToID("_PainterPosition");
    int hardnessID = Shader.PropertyToID("_Hardness");
    int strengthID = Shader.PropertyToID("_Strength");
    int radiusID = Shader.PropertyToID("_Radius");
    int blendOpID = Shader.PropertyToID("_BlendOp");
    int colorID = Shader.PropertyToID("_PainterColor");
    int textureID = Shader.PropertyToID("_MainTex");
    int uvOffsetID = Shader.PropertyToID("_OffsetUV");
    int uvIslandsID = Shader.PropertyToID("_UVIslands");

    Material paintMaterial;
    Material extendMaterial;

    CommandBuffer command;

    public override void Awake(){
        base.Awake();
        UnityEngine.Debug.Log("Paint manager awake");
        paintMaterial = new Material(texturePaint);
        extendMaterial = new Material(extendIslands);
        command = new CommandBuffer();
        command.name = "CommmandBuffer - " + gameObject.name;
    }

	public void enablePaint()
	{
		var collidersObj = colourSelector.gameObject.GetComponentsInChildren<Collider>();
		for (var index = 0; index < collidersObj.Length; index++)
		{
			var colliderItem = collidersObj[index];
			colliderItem.enabled = true;
		}

		//colourSelector.SetActive(true);
		colourSelector.transform.localScale = new Vector3(.4f,.4f,.4f);
	}
	public void disablePaint()
	{
		var collidersObj = colourSelector.gameObject.GetComponentsInChildren<Collider>();
		for (var index = 0; index < collidersObj.Length; index++)
		{
			var colliderItem = collidersObj[index];
			colliderItem.enabled = false;
		}

		colourSelector.transform.localScale = new Vector3(0, 0, 0);
		//colourSelector.SetActive(false);
	}
	public bool isEnabled()
	{
		if(colourSelector.transform.localScale.magnitude == 0) return false;
		return true;
		//return colourSelector.active;
	}

	
	

	public bool newColour()
	{
		if(selectedColour != colourSelector.GetComponentInChildren<ColorPicker>().CurrentColor)
		{
			selectedColour = colourSelector.GetComponentInChildren<ColorPicker>().CurrentColor;
			return true;
		}
		else return false;
	}

    public void initTextures(Paintable paintable){
        RenderTexture mask = paintable.getMask();
        RenderTexture uvIslands = paintable.getUVIslands();
        RenderTexture extend = paintable.getExtend();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        command.SetRenderTarget(mask);
        command.SetRenderTarget(extend);
        command.SetRenderTarget(support);

        paintMaterial.SetFloat(prepareUVID, 1);
        command.SetRenderTarget(uvIslands);
        command.DrawRenderer(rend, paintMaterial, 0);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();
    }


    public void paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? color = null){
        RenderTexture mask = paintable.getMask();
        RenderTexture uvIslands = paintable.getUVIslands();
        RenderTexture extend = paintable.getExtend();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        //UnityEngine.Debug.Log("Inside paint");

        paintMaterial.SetFloat(prepareUVID, 0);
        paintMaterial.SetVector(positionID, pos);
        paintMaterial.SetFloat(hardnessID, hardness);
        paintMaterial.SetFloat(strengthID, strength);
        paintMaterial.SetFloat(radiusID, radius);
        paintMaterial.SetTexture(textureID, support);
        paintMaterial.SetColor(colorID, color ?? Color.red);
        extendMaterial.SetFloat(uvOffsetID, paintable.extendsIslandOffset);
        extendMaterial.SetTexture(uvIslandsID, uvIslands);

        command.SetRenderTarget(mask);
        command.DrawRenderer(rend, paintMaterial, 0);

        command.SetRenderTarget(support);
        command.Blit(mask, support);

        command.SetRenderTarget(extend);
        command.Blit(mask, extend, extendMaterial);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();
    }

}
