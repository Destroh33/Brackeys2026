mergeInto(LibraryManager.library, {
  CopyToClipboard: function (text) {
    var value = UTF8ToString(text);
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(value);
      return;
    }
    var area = document.createElement('textarea');
    area.value = value;
    area.style.position = 'fixed';
    area.style.opacity = '0';
    document.body.appendChild(area);
    area.select();
    try { document.execCommand('copy'); } catch (e) {}
    document.body.removeChild(area);
  }
});
