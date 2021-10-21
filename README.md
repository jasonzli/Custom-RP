# A Custom Render Pipeline for Unity

What's the benefit of having a scriptable render pipeline to a videogame designer? Let's find out as I go through Jasper's comprehensive guide on Unity's pipeline. 

In here, I am following along with Catlike Coding's guide for the forward rendering scriptable pipeline. It is loaded up with my comments. Depending on how this goes, I'll write how it all went and what I think is beneficial about it.


### Rationale

A big unspoken assumption in videogame creation is the engine you use. Go Unreal for HD and Unity for Mobile! But that advice is difficult for developers to parse. Experience and "just go for it" takes the place of real guidance. 

That's absurd! Engines also have baked-in ideas about audiovisual content as well as how that code is allowed to execute. So I'm here looking at Unity's pipeline in an effort to better understand what game designers can gain from understanding how the rendering tool itself influences our designs.

-Jason

## To Do list

- [x] [Setup](https://catlikecoding.com/unity/tutorials/custom-srp/custom-render-pipeline/)
- [ ] [Draw Calls](https://catlikecoding.com/unity/tutorials/custom-srp/draw-calls/)
- [ ] [Directional Lights](https://catlikecoding.com/unity/tutorials/custom-srp/directional-lights/)
- [ ] [Directional Shadows](https://catlikecoding.com/unity/tutorials/custom-srp/directional-shadows/)
- [ ] [Baked Light](https://catlikecoding.com/unity/tutorials/custom-srp/baked-light/)
- [ ] [Shadow Masks](https://catlikecoding.com/unity/tutorials/custom-srp/shadow-masks/)
- [ ] [LOD and Reflections](https://catlikecoding.com/unity/tutorials/custom-srp/lod-and-reflections/)
- [ ] [Complex Maps](https://catlikecoding.com/unity/tutorials/custom-srp/complex-maps/)
- [ ] [Point and Spot Lights](https://catlikecoding.com/unity/tutorials/custom-srp/point-and-spot-lights/)
- [ ] [Point and Spot Shadows](https://catlikecoding.com/unity/tutorials/custom-srp/point-and-spot-shadows/)
- [ ] [Post Processing](https://catlikecoding.com/unity/tutorials/custom-srp/post-processing/)
- [ ] [HDR](https://catlikecoding.com/unity/tutorials/custom-srp/hdr/)
- [ ] [Color Grading](https://catlikecoding.com/unity/tutorials/custom-srp/color-grading/)
- [ ] [Multiple Cameras](https://catlikecoding.com/unity/tutorials/custom-srp/multiple-cameras/)
- [ ] [Particles](https://catlikecoding.com/unity/tutorials/custom-srp/particles/)
- [ ] [Render Scale](https://catlikecoding.com/unity/tutorials/custom-srp/render-scale/)
- [ ] [FXAA](https://catlikecoding.com/unity/tutorials/custom-srp/fxaa/)