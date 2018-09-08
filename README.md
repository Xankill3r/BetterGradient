# BetterGradient

BetterGradient is a small test-bed for generating better looking gradients by converting to the LCH colorspace. It may eventually be used as a plugin within Unity3D, GIMP, Krita, Photoshop, etc.

### Usage

* Browse to [the test page](https://xankill3r.github.io/BetterGradient/).

* Simply choose the colors by clicking on the boxes and these sets of gradients will get generated
  * RGB mix
  * Linear RGB mix
  * LAB mix
  * LCH mix
  * Subtle LCH mix (uses a bunch of Gauss curves)
  * RGB LCH blend mix (blends between RGB and LCH based on hue difference)

* Hovering anywhere within the gradients will show a comparison between these values.

### Based on

Currently uses the [Chrome.js](https://github.com/gka/chroma.js) library for proof of concept. Final versions (Photoshop, Unity, GIMP, etc) will be based on different code.


### Author

BetterGradient is written by Ujwal Kumar.

### License

Released under MIT license.
