# BetterGradient

BetterGradient generates better looking gradients by converting to the LChuv colorspace. OKLAB support to be added soon.

### [Unity3D Package](https://github.com/Xankill3r/BetterGradient/releases/latest)

### [Wiki](https://github.com/Xankill3r/BetterGradient/wiki)

### Testing

* Browse to [the test page](https://xankill3r.github.io/BetterGradient/).

* Simply choose the colors by clicking on the boxes and these sets of gradients will get generated
  * RGB mix
  * Linear RGB mix
  * LAB mix
  * LChuv mix
  * Subtle LChuv mix (uses a bunch of Gauss curves)
  * RGB LChuv blend mix (blends between RGB and LCH based on hue difference)

* Hovering anywhere within the gradients will show a comparison between these values.

### Based on

Currently uses the [Chroma.js](https://github.com/gka/chroma.js) library for proof of concept. Final versions will be based on different code - hopefully optimized using intrinsics where possible.

### Author

BetterGradient is written by Ujwal Kumar.

### License

Released under MIT license.
