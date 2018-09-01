var rgbScaler, rgbLinScaler, lchScaler, labScaler, rgbLCHBlendScaler, subtleLCHScaler;
var color1, color2;
var rgbCtx, rgbLinCtx, lchCtx, subtleLchCtx, labCtx, avgCtx;
var g1RGBGraph, g1LCHGraph;
var g2RGBGraph, g2LCHGraph;
var g3RGBGraph, g3LCHGraph;
var g4RGBGraph, g4LCHGraph;
var g5RGBGraph, g5LCHGraph;
var g6RGBGraph, g6LCHGraph;
function updateGradient() {
    rgbScaler = chroma.scale([color1.value, color2.value]);
    rgbLinScaler = chroma.scale([color1.value, color2.value]).mode("lrgb");
    lchScaler = chroma.scale([color1.value, color2.value]).mode("lch");
    labScaler = chroma.scale([color1.value, color2.value]).mode("lab");
    rgbLCHBlendScaler = new RGBLCHBlendScaler([color1.value, color2.value]);
    subtleLCHScaler = new SubtleLCHScaler([color1.value, color2.value]);
    rgbCtx.clearRect(0, 0, 400, 40);
    rgbLinCtx.clearRect(0, 0, 400, 40);
    lchCtx.clearRect(0, 0, 400, 40);
    subtleLchCtx.clearRect(0, 0, 400, 40);
    labCtx.clearRect(0, 0, 400, 40);
    avgCtx.clearRect(0, 0, 400, 40);

    g1LCHGraph.clearRect(0, 0, 420, 120);
    g1RGBGraph.clearRect(0, 0, 420, 120);
    g2LCHGraph.clearRect(0, 0, 420, 120);
    g2RGBGraph.clearRect(0, 0, 420, 120);
    g3LCHGraph.clearRect(0, 0, 420, 120);
    g3RGBGraph.clearRect(0, 0, 420, 120);
    g4LCHGraph.clearRect(0, 0, 420, 120);
    g4RGBGraph.clearRect(0, 0, 420, 120);
    g5LCHGraph.clearRect(0, 0, 420, 120);
    g5RGBGraph.clearRect(0, 0, 420, 120);
    g6LCHGraph.clearRect(0, 0, 420, 120);
    g6RGBGraph.clearRect(0, 0, 420, 120);
    for (var i = 0; i < 400; i++) {
        updateOutput(rgbCtx, g1RGBGraph, g1LCHGraph, rgbScaler(i / 400.0), i);
        updateOutput(rgbLinCtx, g2RGBGraph, g2LCHGraph, rgbLinScaler(i / 400.0), i);
        updateOutput(lchCtx, g3RGBGraph, g3LCHGraph, lchScaler(i / 400.0), i);
        updateOutput(subtleLchCtx, g4RGBGraph, g4LCHGraph, subtleLCHScaler.blend(i / 400.0), i);
        updateOutput(labCtx, g5RGBGraph, g5LCHGraph, labScaler(i / 400.0), i);
        updateOutput(avgCtx, g6RGBGraph, g6LCHGraph, rgbLCHBlendScaler.blend(i / 400.0), i);
    }
}
function updateOutput(colorCtx, rgbGraphCtx, lchGraphCtx, color, x) {
    // Gradient
    colorCtx.fillStyle = color.hex();
    colorCtx.fillRect(x, 0, 1, 80);
    // RGB Graph
    var rgbVal = color.rgb();
    rgbGraphCtx.fillStyle = "#ff0000";
    rgbGraphCtx.fillRect(x + 10, (1 - (rgbVal[0] / 255.0)) * 100 + 10, 1, 1);
    rgbGraphCtx.fillStyle = "#00ff00";
    rgbGraphCtx.fillRect(x + 10, (1 - (rgbVal[1] / 255.0)) * 100 + 10, 1, 1);
    rgbGraphCtx.fillStyle = "#0000ff";
    rgbGraphCtx.fillRect(x + 10, (1 - (rgbVal[2] / 255.0)) * 100 + 10, 1, 1);
    // LCH Graph
    var lchVal = color.lch();
    lchGraphCtx.fillStyle = "#000000";
    lchGraphCtx.fillRect(x + 10, (1 - (lchVal[0] / 100.0)) * 100 + 10, 1, 1);
    lchGraphCtx.fillStyle = "#aaaa00";
    lchGraphCtx.fillRect(x + 10, (1 - (lchVal[1] / 100.0)) * 100 + 10, 1, 1);
    lchGraphCtx.fillStyle = "#ff00ff";
    lchGraphCtx.fillRect(x + 10, (1 - (lchVal[2] / 360.0)) * 100 + 10, 1, 1);
}

function lerp(v0, v1, t) {
    return (1 - t) * v0 + t * v1;
}

class RGBLCHBlendScaler {
    constructor(colors) {
        // Gauss curve
        this.rgbLCHWeightCurve = gaussian(0, 0.005);
        var rgbLCHWeightPeak = 0.9;
        this.rgbLCHWeightScale = (1 / this.rgbLCHWeightCurve.pdf(0)) * rgbLCHWeightPeak;
        // Scalers
        this.rgbScaler = chroma.scale(colors);
        this.lchScaler = chroma.scale(colors).mode("lch");
        var start = this.lchScaler(0);
        var end = this.lchScaler(1);
        var hueDistance = Math.abs(start.lch()[2] - end.lch()[2]);
        if (isNaN(hueDistance)) hueDistance = 0;
        if (hueDistance > 180) hueDistance = 360 - hueDistance;
        this.blendWeight = this.rgbLCHBlendWeight(hueDistance);
    }
    blend(t) {
        var rawLCH = this.lchScaler(t).hex();
        var rawRGB = this.rgbScaler(t).hex();
        return chroma.mix(rawLCH, rawRGB, this.blendWeight, "lab");
    }
    rgbLCHBlendWeight(hueDistance) {
        hueDistance /= 180.0;
        return this.rgbLCHWeightScale * this.rgbLCHWeightCurve.pdf(1 - hueDistance);
    }
}

class SubtleLCHScaler {
    constructor(colors) {
        // Luma Gauss curve
        this.subtleLumaCurve = gaussian(0, 0.6);
        this.subtleLumaMin = this.subtleLumaCurve.pdf(1);
        this.subtleLumaScale = 1 / (this.subtleLumaCurve.pdf(0) - this.subtleLumaMin);
        // Chroma Gauss curve
        this.subtleChromaCurve = gaussian(0, 0.8);
        this.subtleChromaMin = this.subtleChromaCurve.pdf(1);
        this.subtleChromaScale = 1 / (this.subtleChromaCurve.pdf(0) - this.subtleChromaMin);
        // Scaler
        this.lchScaler = chroma.scale(colors).mode("lch");
        var start = this.lchScaler(0);
        var end = this.lchScaler(1);
        var hueDistance = Math.abs(start.lch()[2] - end.lch()[2]);
        if (isNaN(hueDistance)) hueDistance = 0;
        if (hueDistance > 180) hueDistance = 360 - hueDistance;
        hueDistance /= 180.0;
        var distanceToWeightCurve = gaussian(0, 0.01);
        var dToWeightScale = (1 / distanceToWeightCurve.pdf(0)) * 0.65;
        this.blendWeight = dToWeightScale * distanceToWeightCurve.pdf(1 - hueDistance);
    }
    blend(t) {
        var raw = this.lchScaler(t).lch();
        var l = this.subtleLuma(raw[0], this.blendWeight, t);
        var c = this.subtleChroma(raw[1], this.blendWeight, t);
        return chroma(l, c, raw[2], "lch");
    }
    subtleLuma(raw, weight, t) {
        t = this.subtleLumaScale * (this.subtleLumaCurve.pdf(t * 2 - 1) - this.subtleLumaMin);
        t = weight * t * 0.2;
        return lerp(raw, 0, t);
    }
    subtleChroma(raw, weight, t) {
        t = this.subtleChromaScale * (this.subtleChromaCurve.pdf(t * 2 - 1) - this.subtleChromaMin);
        t = weight * t * 1;
        return lerp(raw, 0, t);
    }
}

var g1rgb, g1lch, g2rgb, g2lch, g3rgb, g3lch, g4rgb, g4lch, g5rgb, g5lch, g6rgh, g6lch;
function compareColors(ev) {
    var x = ev.clientX - 5;
    var rect = ev.target.getBoundingClientRect();
    x -= rect.left;
    var rgb = rgbScaler(x / 400.0);
    var lrgb = rgbLinScaler(x / 400.0);
    var lch = lchScaler(x / 400.0);
    var subtle = subtleLCHScaler.blend(x / 400.0);
    var lab = labScaler(x / 400.0);
    var avg = rgbLCHBlendScaler.blend(x / 400.0);
    g1rgb.innerHTML = arr3ToString(rgb.rgb());
    g1lch.innerHTML = arr3ToString(rgb.lch());
    g2rgb.innerHTML = arr3ToString(lrgb.rgb());
    g2lch.innerHTML = arr3ToString(lrgb.lch());
    g3rgb.innerHTML = arr3ToString(lch.rgb());
    g3lch.innerHTML = arr3ToString(lch.lch());
    g4rgb.innerHTML = arr3ToString(subtle.rgb());
    g4lch.innerHTML = arr3ToString(subtle.lch());
    g5rgb.innerHTML = arr3ToString(lab.rgb());
    g5lch.innerHTML = arr3ToString(lab.lch());
    g6rgb.innerHTML = arr3ToString(avg.rgb());
    g6lch.innerHTML = arr3ToString(avg.lch());
}
function arr3ToString(arr) {
    var n1 = +arr[0].toFixed(2);
    var n2 = +arr[1].toFixed(2);
    var n3 = +arr[2].toFixed(2);
    return n1 + ", " + n2 + ", " + n3;
}

function init() {
    color1 = document.getElementById("c1");
    color2 = document.getElementById("c2");

    rgbCtx = document.getElementById("gradient1").getContext("2d");
    rgbLinCtx = document.getElementById("gradient2").getContext("2d");
    lchCtx = document.getElementById("gradient3").getContext("2d");
    subtleLchCtx = document.getElementById("gradient4").getContext("2d");
    labCtx = document.getElementById("gradient5").getContext("2d");
    avgCtx = document.getElementById("gradient6").getContext("2d");
    
    g1RGBGraph = document.getElementById("g1rgbGraph").getContext("2d");
    g1LCHGraph = document.getElementById("g1lchGraph").getContext("2d");
    g2RGBGraph = document.getElementById("g2rgbGraph").getContext("2d");
    g2LCHGraph = document.getElementById("g2lchGraph").getContext("2d");
    g3RGBGraph = document.getElementById("g3rgbGraph").getContext("2d");
    g3LCHGraph = document.getElementById("g3lchGraph").getContext("2d");
    g4RGBGraph = document.getElementById("g4rgbGraph").getContext("2d");
    g4LCHGraph = document.getElementById("g4lchGraph").getContext("2d");
    g5RGBGraph = document.getElementById("g5rgbGraph").getContext("2d");
    g5LCHGraph = document.getElementById("g5lchGraph").getContext("2d");
    g6RGBGraph = document.getElementById("g6rgbGraph").getContext("2d");
    g6LCHGraph = document.getElementById("g6lchGraph").getContext("2d");

    g1rgb = document.getElementById("g1rgb");
    g1lch = document.getElementById("g1lch");
    g2rgb = document.getElementById("g2rgb");
    g2lch = document.getElementById("g2lch");
    g3rgb = document.getElementById("g3rgb");
    g3lch = document.getElementById("g3lch");
    g4rgb = document.getElementById("g4rgb");
    g4lch = document.getElementById("g4lch");
    g5rgb = document.getElementById("g5rgb");
    g5lch = document.getElementById("g5lch");
    g6rgb = document.getElementById("g6rgb");
    g6lch = document.getElementById("g6lch");

    updateGradient();
}
