mergeInto(LibraryManager.library, {
  CopyToClipboard: function (text) {
    var value = UTF8ToString(text);

    var legacy = function () {
      var area = document.createElement('textarea');
      area.value = value;
      area.setAttribute('readonly', '');
      area.style.position = 'fixed';
      area.style.top = '0';
      area.style.left = '0';
      area.style.width = '1px';
      area.style.height = '1px';
      area.style.padding = '0';
      area.style.border = 'none';
      area.style.outline = 'none';
      area.style.boxShadow = 'none';
      area.style.background = 'transparent';
      area.style.opacity = '0';
      document.body.appendChild(area);

      var active = document.activeElement;
      var ok = false;
      try {
        area.focus();
        area.select();
        area.setSelectionRange(0, value.length);
        ok = document.execCommand('copy');
      } catch (e) {
        ok = false;
      }

      document.body.removeChild(area);
      if (active && active.focus) {
        try { active.focus(); } catch (e) {}
      }
      return ok;
    };

    if (legacy()) return;

    try {
      if (navigator.clipboard && navigator.clipboard.writeText) {
        var p = navigator.clipboard.writeText(value);
        if (p && p.catch) p.catch(function () {});
      }
    } catch (e) {}
  }
});
