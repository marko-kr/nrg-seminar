// #package glsl/mixins

// #section Photon
//CHANGE add density float
struct Photon {
    vec3 position;
    vec3 direction;
    vec3 transmittance;
    vec3 radiance;
    uint bounces;
    uint samples;
    float density;
};
