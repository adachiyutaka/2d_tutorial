mergeInto(LibraryManager.library, {

  // 関数呼び出し
  Hello: function () {
    window.alert("Hello, world!");
  },

  // 数値型の引数と戻り値
  AddNumbers: function (x, y) {
    return x + y;
  },

  // 数値型以外の型の引数
  PrintFloatArray: function (array, size) {
    for(var i = 0; i < size; i++)
      console.log(HEAPF32[(array >> 2) + i]);
  },

  // 文字列型の引数
  HelloString: function (str) {
    window.alert(Pointer_stringify(str));
  },

  // 文字列の戻り値
  StringReturnValueFunction: function () {
    var returnStr = location.pathname.replace('/games/', '');
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  // WebGLテクスチャのバインド
  BindWebGLTexture: function (texture) {
    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texture]);
  },
});