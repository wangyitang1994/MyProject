

class SldShowByValue:SldShowFunction
{
    private float perValue;
    private SldBar curSld;

    public override void Init(SldBar sld,int per){
        curSld = sld;
        perValue = per;
    }

    public override void SetValue(float value){
        
    }
}