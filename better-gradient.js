var rgbScaler, rgbLinScaler, lchScaler, labScaler;
var color1, color2;
var rgbCtx, rgbLinCtx, lchCtx, subtleLchCtx, labCtx, avgCtx;
function updateGradient() {
    rgbScaler = chroma.scale([color1.value, color2.value]);
    rgbLinScaler = chroma.scale([color1.value, color2.value]).mode("lrgb");
    lchScaler = chroma.scale([color1.value, color2.value]).mode("lch");
    labScaler = chroma.scale([color1.value, color2.value]).mode("lab");
    for (var i = 0; i < 400; i++) {
        rgbCtx.fillStyle = rgbScaler(i / 400.0).hex();
        rgbCtx.fillRect(i, 0, 1, 80);
        rgbLinCtx.fillStyle = rgbLinScaler(i / 400.0).hex();
        rgbLinCtx.fillRect(i, 0, 1, 80);
        lchCtx.fillStyle = lchScaler(i / 400.0).hex();
        lchCtx.fillRect(i, 0, 1, 80);
        subtleLchCtx.fillStyle = subtleLch(lchScaler, i / 400.0).hex();
        subtleLchCtx.fillRect(i, 0, 1, 80);
        labCtx.fillStyle = labScaler(i / 400.0).hex();
        labCtx.fillRect(i, 0, 1, 80);
        avgCtx.fillStyle = rgbLCHBlend(rgbScaler, lchScaler, i / 400.0).hex();
        avgCtx.fillRect(i, 0, 1, 80);
    }
}

function lerp(a, b, t) {
    return a + (b - a) * t;
}

function rgbLCHBlend(rgbScaler, lchScaler, t) {
    var start = lchScaler(0);
    var end = lchScaler(1);
    var hueDistance = Math.abs(start.lch()[2] - end.lch()[2]);
    if (hueDistance > 180) hueDistance = 360 - hueDistance;
    var blendWeight = rgbLCHBlendWeight(hueDistance);
    var rawLCH = lchScaler(t).hex();
    var rawRGB = rgbScaler(t).hex();
    return chroma.mix(rawLCH, rawRGB, blendWeight, "lab");
}
var rgbLCHWeightCurve = gaussian(0, 0.005);
var rgbLCHWeightPeak = 0.4;
var rgbLCHWeightScale = (1 / rgbLCHWeightCurve.pdf(0)) * rgbLCHWeightPeak;
function rgbLCHBlendWeight(hueDistance) {
    hueDistance /= 180.0;
    return rgbLCHWeightScale * rgbLCHWeightCurve.pdf(1 - hueDistance);
}

function subtleLch(scaler, t) {
    var start = scaler(0);
    var end = scaler(1);
    var raw = scaler(t).lch();
    var hueDistance = Math.abs(start.lch()[2] - end.lch()[2]);
    if (hueDistance > 180) hueDistance = 360 - hueDistance;
    var weight = hueDistanceToWeight(hueDistance);
    var l = subtleLuma(raw[0], weight, t);
    var c = subtleChroma(raw[1], weight, t);
    return chroma(l, c, raw[2], "lch");
}
var distanceToWeightCurve = gaussian(0, 0.01);
var dToWeightScale = (1 / distanceToWeightCurve.pdf(0)) * 0.65;
function hueDistanceToWeight(distance) {
    distance /= 180.0;
    return dToWeightScale * distanceToWeightCurve.pdf(1 - distance);
}
var subtleLumaCurve = gaussian(0, 0.6);
var subtleLumaMin = subtleLumaCurve.pdf(1);
var subtleLumaScale = 1 / (subtleLumaCurve.pdf(0) - subtleLumaMin);
/**
 * Calculates subtle Luma value from the raw value at location t [0, 1] along the gradient
 */
function subtleLuma(raw, weight, t) {
    t = subtleLumaScale * (subtleLumaCurve.pdf(t * 2 - 1) - subtleLumaMin);
    t = weight * t * 0.2;
    return lerp(raw, 0, t);
}
var subtleChromaCurve = gaussian(0, 0.8);
var subtleChromaMin = subtleChromaCurve.pdf(1);
var subtleChromaScale = 1 / (subtleChromaCurve.pdf(0) - subtleChromaMin);
/**
 * Calculates subtle Chroma value from the raw value at location t [0, 1] along the gradient
 */
function subtleChroma(raw, weight, t) {
    t = subtleChromaScale * (subtleChromaCurve.pdf(t * 2 - 1) - subtleChromaMin);
    t = weight * t * 1;
    return lerp(raw, 0, t);
}

var g1rgb, g1lch, g2rgb, g2lch, g3rgb, g3lch, g4rgb, g4lch, g5rgb, g5lch, g6rgh, g6lch;
function compareColors(ev) {
    var x = ev.clientX - 5;
    var rect = ev.target.getBoundingClientRect();
    x -= rect.left;
    var rgb = rgbScaler(x / 400.0);
    var lrgb = rgbLinScaler(x / 400.0);
    var lch = lchScaler(x / 400.0);
    var subtle = subtleLch(lchScaler, x / 400.0);
    var lab = labScaler(x / 400.0);
    var avg = rgbLCHBlend(rgbScaler, lchScaler, x / 400.0);
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
