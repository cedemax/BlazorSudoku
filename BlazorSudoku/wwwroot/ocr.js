

async function recognize(i) {
    const chars = document.querySelector("#sudokuGame").getAttribute("data-chars");
    const video = document.querySelector("#video");
    const k = video.width / chars.length;
    let res = "";
    const canvas = document.querySelector("#canvas");
    const ctx = canvas.getContext('2d');
    const canvas2 = document.querySelector("#canvas2");
    const ctx2 = canvas2.getContext('2d');


    for (var y = 0; y < chars.length; ++y) {
        for (var x = 0; x < chars.length; ++x) {

            var x0 = x * k - 0.15*k;
            var x1 = (x + 1) * k + 0.15 * k;
            var y0 = y * k - 0.15 * k;
            var y1 = (y + 1) * k + 0.15 * k;

            const image = ctx.getImageData(x0, y0, x1 - x0, y1 - y0);
            ctx2.putImageData(image, 0, 0);

            console.log("processing:" + x0 + "-" + x1 + " " + y0 + "-" + y1);

            let texts = OCRAD(ctx2, {
                filters: ["numbers_only"]
            })
            console.log(texts);

            texts = [...texts].map(x => chars.indexOf(x) < 0 ? "" : x).join("")
            if (texts.length > 1) {
                var firstNon1 = [...texts].findIndex(x => x != "1");
                texts = texts[Math.max(0,firstNon1)];
            }
            
            console.log(x + " , " + y + " = " + texts);
            //console.log(data);
            res = res + (texts.length == 0?" ":texts);
        }
        res = res + "\n";
    }
    console.log(res);
    //var syms = data.data.symbols.filter(x => x.confidence > 50 && x.text != " ");
    //var ocrResult = syms.map(sym => {
    //    var cx = (sym.bbox.x0 + sym.bbox.x1) / 2;
    //    var cy = (sym.bbox.y0 + sym.bbox.y1) / 2;
    //    var X = chars.length * cx / 600;
    //    var Y = chars.length * cy / 600;
    //    return { value: sym.text, x: X, y: Y };
    //});
    //GLOBAL.DotNetReference.invokeMethodAsync('SetOCRResult', { result: ocrResult })
}



async function startCamera() {
    console.log("Starting Camera");
    let stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
    var video = document.querySelector("#video");
    var sudoku = document.querySelector(".sudoku");
    const rect = sudoku.getBoundingClientRect();
    var sx = rect.left;
    var sy = rect.top;
    var sw = sudoku.clientWidth;
    var sh = sudoku.clientWidth;
    video.srcObject = stream;
    overlay(video);
    document.querySelector(".popup").classList.add("active");
}
function takePhoto() {
    var canvas = document.querySelector("#canvas");
    const ctx = canvas.getContext('2d');
    var video = document.querySelector("#video");
    ctx.drawImage(video, 0, 0, video.videoHeight, video.videoHeight, 0, 0, canvas.width, canvas.width);
    ctx.putImageData(preprocessImage(canvas), 0, 0);
    image_data_url = canvas.toDataURL('image/jpeg');
    overlay(canvas);
    recognize();
}

function overlay(el) {
    var sudoku = document.querySelector(".sudoku");
    const rect = sudoku.getBoundingClientRect();
    var sx = rect.left;
    var sy = rect.top;
    var sw = sudoku.clientWidth;
    var sh = sudoku.clientWidth;
    el.setAttribute("style", "width:" + sw + "px;height:" + sh + "px;left:" + sx + "px;top:" + sy + "px;");
}

let thr = 0.27;
function preprocessImage(canvas) {
    var video = document.querySelector("#video");
    const ctx = canvas.getContext('2d');
    
    const image = ctx.getImageData(0, 0, canvas.width, canvas.width);
    bw(image.data);
    thrsh(image.data,canvas);
    //thresholdFilter(image.data, thr);
    //dilate(image.data, canvas);
    //invertColors(image.data);
    //dilate(image.data, canvas);
    //invertColors(image.data);
    //dilate(image.data, canvas);
    //dilate(image.data, canvas);
    //invertColors(image.data);
    return image;
}

//from https://github.com/processing/p5.js/blob/main/src/image/filters.js

function bw(pixels) {
    for (let i = 0; i < pixels.length; i += 4) {
        const red = pixels[i];
        const green = pixels[i + 1];
        const blue = pixels[i + 2];
        const gray = 0.2126 * red + 0.7152 * green + 0.0722 * blue;
        pixels[i] = pixels[i + 1] = pixels[i + 2] = gray;
    }
}

function thrsh(pixels, canvas) {
    var w = canvas.width;
    var h = canvas.height;
    const out0 = new Int32Array(pixels.length / 4);
    const out1 = new Int32Array(pixels.length / 4);
    const r = 16;
    for (var x = 0; x < w; ++x) {
        for (var y = 0; y < w; ++y) {
            var i = x + y * w;
            var gray = pixels[i * 4];
            var min = Math.max(30,gray);
            var max = Math.min(150,gray);
            for (var dx = -r; dx <= r; ++dx) {
                var xx = x + dx;
                if (xx < 0 || xx >= w)
                    continue;
                for (var dy = -r; dy <= r; ++dy) {
                    var yy = y + dy;
                    if (yy < 0 || yy >= h)
                        continue;
                    var j = xx + yy * w;
                    const gray2 = pixels[j*4];
                    min = Math.min(min, gray2);
                    max = Math.max(max, gray2);
                }
            }
            out0[i] = min;
            out1[i] = max;
        }
    }
    for (let i = 0; i < pixels.length/4; i ++)
    {
        var isLow = pixels[i * 4] < out0[i] * 2;
        var isHigh = pixels[i * 4] > out1[i] * 0.7;
        out0[i] = isLow && !isHigh ? 0 : 1;
    }
    const r2 = 7;
    const lim2 = (r2 * 2 + 1) * (r2 * 2 + 1) - 70;
    for (var iter = 0; iter < 0; ++iter) {
        for (var x = 0; x < w; ++x) {
            for (var y = 0; y < w; ++y) {
                var i = x + y * w;
                var A = out0[i];
                var sumy = A;
                for (var dx = -r2; dx <= r2; ++dx) {
                    var xx = x + dx;
                    if (xx < 0 || xx >= w)
                        continue;
                    for (var dy = -r2; dy <= r2; ++dy) {
                        var yy = y + dy;
                        if (yy < 0 || yy >= h)
                            continue;
                        var j = xx + yy * w;
                        var B = out0[j];
                        sumy = sumy + B;
                    }
                }

                out1[i] = (A == 0 && sumy < lim2) ? 0 : 1;
            }
        }
        for (var i = 0; i < pixels.length / 4; ++i)
            out0[i] = out1[i];
    }


    const r3 = 4;
    //perform(out0, out1, w, h, 1,(v) => v, (t, v) => Math.min(t, v), (t, o) => t);
    //perform(out0, out1, w, h, r3,(v) => 0, (t, v) => t+(1-v), (t, o) => t);
    //perform(out0, out1, w, h, r3, (v) => v, (t, v) => Math.max(t,v), (t, o) => t);
    for (var iter = 0; iter < 4; ++iter) {
        for (var x = 0; x < w; ++x) {
            for (var y = 0; y < w; ++y) {
                var i = x + y * w;
                var A = out0[i];
                var sum = 0;
                for (var dx = -r3; dx <= r3; ++dx) {
                    var xx = x + dx;
                    if (xx < 0 || xx >= w)
                        continue;
                    for (var dy = -r3; dy <= r3; ++dy) {
                        var yy = y + dy;
                        if (yy < 0 || yy >= h)
                            continue;
                        var j = xx + yy * w;
                        var B = out0[j];
                        sum = sum + (1 - B);
                    }
                }
                out1[i] = sum;
            }
        }
        const lim3 = (r3 * 2 + 1) * (r3 * 2 + 1) * 0.33;
        for (var x = 0; x < w; ++x) {
            for (var y = 0; y < w; ++y) {
                var i = x + y * w;
                var A = out1[i];
                var max = A;
                for (var dx = -r3; dx <= r3; ++dx) {
                    var xx = x + dx;
                    if (xx < 0 || xx >= w)
                        continue;
                    for (var dy = -r3; dy <= r3; ++dy) {
                        var yy = y + dy;
                        if (yy < 0 || yy >= h)
                            continue;
                        var j = xx + yy * w;
                        var B = out1[j];
                        max = Math.max(max, B);
                    }
                }
                out0[i] = out0[i] == 0 && max >= lim3 ? 0 : 1;
            }
        }
    }
    
    for (var i = 0; i < pixels.length / 4; ++i)
        out0[i] = out0[i] == 0 ? 0xFF000000 : 0xFFFFFFFF;
    setPixels(pixels, out0);
}

function perform(out0, out1, w, h, r,v0,fun,agg) {
    for (var x = 0; x < w; ++x) {
        for (var y = 0; y < w; ++y) {
            var i = x + y * w;
            var A = out0[i];
            var tmp = v0(A);
            for (var dx = -r; dx <= r; ++dx) {
                var xx = x + dx;
                if (xx < 0 || xx >= w)
                    continue;
                for (var dy = -r; dy <= r; ++dy) {
                    var yy = y + dy;
                    if (yy < 0 || yy >= h)
                        continue;
                    var j = xx + yy * w;
                    var B = out0[j];
                    tmp = fun(tmp,B);
                }
            }
            out1[i] = agg(tmp,out0[i]);
        }
    }
    for (var i = 0; i < w*h; ++i)
        out0[i] = out1[i];
}


function thresholdFilter(pixels, level) {
    if (level === undefined) {
        level = 0.5;
    }
    const thresh = Math.floor(level * 255);
    for (let i = 0; i < pixels.length; i += 4) {
        const red = pixels[i];
        const green = pixels[i + 1];
        const blue = pixels[i + 2];

        const gray = 0.2126 * red + 0.7152 * green + 0.0722 * blue;
        let value;
        if (gray >= thresh) {
            value = 255;
        } else {
            value = 0;
        }
        pixels[i] = pixels[i + 1] = pixels[i + 2] = value;
    }
}


function getARGB(data, i) {
    const offset = i * 4;
    return (
        ((data[offset + 3] << 24) & 0xff000000) |
        ((data[offset] << 16) & 0x00ff0000) |
        ((data[offset + 1] << 8) & 0x0000ff00) |
        (data[offset + 2] & 0x000000ff)
    );
};

function setPixels(pixels, data) {
    let offset = 0;
    for (let i = 0, al = pixels.length; i < al; i++) {
        offset = i * 4;
        pixels[offset + 0] = (data[i] & 0x00ff0000) >>> 16;
        pixels[offset + 1] = (data[i] & 0x0000ff00) >>> 8;
        pixels[offset + 2] = data[i] & 0x000000ff;
        pixels[offset + 3] = (data[i] & 0xff000000) >>> 24;
    }
};


// internal kernel stuff for the gaussian blur filter
let blurRadius;
let blurKernelSize;
let blurKernel;
let blurMult;

// from https://github.com/processing/p5.js/blob/main/src/image/filters.js
function buildBlurKernel(r) {
    let radius = (r * 3.5) | 0;
    radius = radius < 1 ? 1 : radius < 248 ? radius : 248;

    if (blurRadius !== radius) {
        blurRadius = radius;
        blurKernelSize = (1 + blurRadius) << 1;
        blurKernel = new Int32Array(blurKernelSize);
        blurMult = new Array(blurKernelSize);
        for (let l = 0; l < blurKernelSize; l++) {
            blurMult[l] = new Int32Array(256);
        }

        let bk, bki;
        let bm, bmi;

        for (let i = 1, radiusi = radius - 1; i < radius; i++) {
            blurKernel[radius + i] = blurKernel[radiusi] = bki = radiusi * radiusi;
            bm = blurMult[radius + i];
            bmi = blurMult[radiusi--];
            for (let j = 0; j < 256; j++) {
                bm[j] = bmi[j] = bki * j;
            }
        }
        bk = blurKernel[radius] = radius * radius;
        bm = blurMult[radius];

        for (let k = 0; k < 256; k++) {
            bm[k] = bk * k;
        }
    }
}

// from https://github.com/processing/p5.js/blob/main/src/image/filters.js
function blurARGB(pixels, canvas, radius) {
    const width = canvas.width;
    const height = canvas.height;
    const numPackedPixels = width * height;
    const argb = new Int32Array(numPackedPixels);
    for (let j = 0; j < numPackedPixels; j++) {
        argb[j] = getARGB(pixels, j);
    }
    let sum, cr, cg, cb, ca;
    let read, ri, ym, ymi, bk0;
    const a2 = new Int32Array(numPackedPixels);
    const r2 = new Int32Array(numPackedPixels);
    const g2 = new Int32Array(numPackedPixels);
    const b2 = new Int32Array(numPackedPixels);
    let yi = 0;
    buildBlurKernel(radius);
    let x, y, i;
    let bm;
    for (y = 0; y < height; y++) {
        for (x = 0; x < width; x++) {
            cb = cg = cr = ca = sum = 0;
            read = x - blurRadius;
            if (read < 0) {
                bk0 = -read;
                read = 0;
            } else {
                if (read >= width) {
                    break;
                }
                bk0 = 0;
            }
            for (i = bk0; i < blurKernelSize; i++) {
                if (read >= width) {
                    break;
                }
                const c = argb[read + yi];
                bm = blurMult[i];
                ca += bm[(c & -16777216) >>> 24];
                cr += bm[(c & 16711680) >> 16];
                cg += bm[(c & 65280) >> 8];
                cb += bm[c & 255];
                sum += blurKernel[i];
                read++;
            }
            ri = yi + x;
            a2[ri] = ca / sum;
            r2[ri] = cr / sum;
            g2[ri] = cg / sum;
            b2[ri] = cb / sum;
        }
        yi += width;
    }
    yi = 0;
    ym = -blurRadius;
    ymi = ym * width;
    for (y = 0; y < height; y++) {
        for (x = 0; x < width; x++) {
            cb = cg = cr = ca = sum = 0;
            if (ym < 0) {
                bk0 = ri = -ym;
                read = x;
            } else {
                if (ym >= height) {
                    break;
                }
                bk0 = 0;
                ri = ym;
                read = x + ymi;
            }
            for (i = bk0; i < blurKernelSize; i++) {
                if (ri >= height) {
                    break;
                }
                bm = blurMult[i];
                ca += bm[a2[read]];
                cr += bm[r2[read]];
                cg += bm[g2[read]];
                cb += bm[b2[read]];
                sum += blurKernel[i];
                ri++;
                read += width;
            }
            argb[x + yi] =
                ((ca / sum) << 24) |
                ((cr / sum) << 16) |
                ((cg / sum) << 8) |
                (cb / sum);
        }
        yi += width;
        ymi += width;
        ym++;
    }
    setPixels(pixels, argb);
}

function invertColors(pixels) {
    for (var i = 0; i < pixels.length; i += 4) {
        pixels[i] = pixels[i] ^ 255; // Invert Red
        pixels[i + 1] = pixels[i + 1] ^ 255; // Invert Green
        pixels[i + 2] = pixels[i + 2] ^ 255; // Invert Blue
    }
}

// from https://github.com/processing/p5.js/blob/main/src/image/filters.js
function blurARGB(pixels, canvas, radius) {
    const width = canvas.width;
    const height = canvas.height;
    const numPackedPixels = width * height;
    const argb = new Int32Array(numPackedPixels);
    for (let j = 0; j < numPackedPixels; j++) {
        argb[j] = getARGB(pixels, j);
    }
    let sum, cr, cg, cb, ca;
    let read, ri, ym, ymi, bk0;
    const a2 = new Int32Array(numPackedPixels);
    const r2 = new Int32Array(numPackedPixels);
    const g2 = new Int32Array(numPackedPixels);
    const b2 = new Int32Array(numPackedPixels);
    let yi = 0;
    buildBlurKernel(radius);
    let x, y, i;
    let bm;
    for (y = 0; y < height; y++) {
        for (x = 0; x < width; x++) {
            cb = cg = cr = ca = sum = 0;
            read = x - blurRadius;
            if (read < 0) {
                bk0 = -read;
                read = 0;
            } else {
                if (read >= width) {
                    break;
                }
                bk0 = 0;
            }
            for (i = bk0; i < blurKernelSize; i++) {
                if (read >= width) {
                    break;
                }
                const c = argb[read + yi];
                bm = blurMult[i];
                ca += bm[(c & -16777216) >>> 24];
                cr += bm[(c & 16711680) >> 16];
                cg += bm[(c & 65280) >> 8];
                cb += bm[c & 255];
                sum += blurKernel[i];
                read++;
            }
            ri = yi + x;
            a2[ri] = ca / sum;
            r2[ri] = cr / sum;
            g2[ri] = cg / sum;
            b2[ri] = cb / sum;
        }
        yi += width;
    }
    yi = 0;
    ym = -blurRadius;
    ymi = ym * width;
    for (y = 0; y < height; y++) {
        for (x = 0; x < width; x++) {
            cb = cg = cr = ca = sum = 0;
            if (ym < 0) {
                bk0 = ri = -ym;
                read = x;
            } else {
                if (ym >= height) {
                    break;
                }
                bk0 = 0;
                ri = ym;
                read = x + ymi;
            }
            for (i = bk0; i < blurKernelSize; i++) {
                if (ri >= height) {
                    break;
                }
                bm = blurMult[i];
                ca += bm[a2[read]];
                cr += bm[r2[read]];
                cg += bm[g2[read]];
                cb += bm[b2[read]];
                sum += blurKernel[i];
                ri++;
                read += width;
            }
            argb[x + yi] =
                ((ca / sum) << 24) |
                ((cr / sum) << 16) |
                ((cg / sum) << 8) |
                (cb / sum);
        }
        yi += width;
        ymi += width;
        ym++;
    }
    setPixels(pixels, argb);
}

// from https://github.com/processing/p5.js/blob/main/src/image/filters.js
function dilate(pixels, canvas) {
    let currIdx = 0;
    const maxIdx = pixels.length ? pixels.length / 4 : 0;
    const out = new Int32Array(maxIdx);
    let currRowIdx, maxRowIdx, colOrig, colOut, currLum;

    let idxRight, idxLeft, idxUp, idxDown;
    let colRight, colLeft, colUp, colDown;
    let lumRight, lumLeft, lumUp, lumDown;

    while (currIdx < maxIdx) {
        currRowIdx = currIdx;
        maxRowIdx = currIdx + canvas.width;
        while (currIdx < maxRowIdx) {
            colOrig = colOut = getARGB(pixels, currIdx);
            idxLeft = currIdx - 1;
            idxRight = currIdx + 1;
            idxUp = currIdx - canvas.width;
            idxDown = currIdx + canvas.width;

            if (idxLeft < currRowIdx) {
                idxLeft = currIdx;
            }
            if (idxRight >= maxRowIdx) {
                idxRight = currIdx;
            }
            if (idxUp < 0) {
                idxUp = 0;
            }
            if (idxDown >= maxIdx) {
                idxDown = currIdx;
            }
            colUp = getARGB(pixels, idxUp);
            colLeft = getARGB(pixels, idxLeft);
            colDown = getARGB(pixels, idxDown);
            colRight = getARGB(pixels, idxRight);

            //compute luminance
            currLum =
                77 * ((colOrig >> 16) & 0xff) +
                151 * ((colOrig >> 8) & 0xff) +
                28 * (colOrig & 0xff);
            lumLeft =
                77 * ((colLeft >> 16) & 0xff) +
                151 * ((colLeft >> 8) & 0xff) +
                28 * (colLeft & 0xff);
            lumRight =
                77 * ((colRight >> 16) & 0xff) +
                151 * ((colRight >> 8) & 0xff) +
                28 * (colRight & 0xff);
            lumUp =
                77 * ((colUp >> 16) & 0xff) +
                151 * ((colUp >> 8) & 0xff) +
                28 * (colUp & 0xff);
            lumDown =
                77 * ((colDown >> 16) & 0xff) +
                151 * ((colDown >> 8) & 0xff) +
                28 * (colDown & 0xff);

            if (lumLeft > currLum) {
                colOut = colLeft;
                currLum = lumLeft;
            }
            if (lumRight > currLum) {
                colOut = colRight;
                currLum = lumRight;
            }
            if (lumUp > currLum) {
                colOut = colUp;
                currLum = lumUp;
            }
            if (lumDown > currLum) {
                colOut = colDown;
                currLum = lumDown;
            }
            out[currIdx++] = colOut;
        }
    }
    setPixels(pixels, out);
};