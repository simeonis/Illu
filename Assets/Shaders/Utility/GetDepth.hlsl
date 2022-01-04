#ifndef GetDepth_INCLUDED
#define GetDepth_INCLUDED

void GetDepth_float(float UV, out float Out){

    Out = SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV);

};

#endif