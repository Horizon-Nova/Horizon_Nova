// Card Swap：純 JS 版本（非 React）
(function () {
  'use strict';

  document.addEventListener('DOMContentLoaded', function () {
    var root = document.getElementById('CardSwap');
    if (!root) return;

    var cards = Array.prototype.slice.call(root.querySelectorAll('.hn-card'));
    if (cards.length < 2) return;

    var cardDistance = 60;
    var verticalDistance = 70;
    var intervalMs = 8000; // 每 8 秒自動換一張
    var skewAmount = 6;

    function makeSlot(i, distX, distY, total) {
      return {
        x: i * distX,
        y: -i * distY,
        z: -i * distX * 1.5,
        zIndex: total - i
      };
    }

    function placeNow(el, slot, skew) {
      el.style.transform =
        'translate3d(' +
        slot.x +
        'px,' +
        slot.y +
        'px,' +
        slot.z +
        'px) translate(-50%, -50%) skewY(' +
        skew +
        'deg)';
      el.style.zIndex = String(slot.zIndex);
      el.style.opacity = '1';
    }

    var order = [];
    for (var i = 0; i < cards.length; i++) {
      order.push(i);
    }

    function layoutAllNow() {
      var total = cards.length;
      for (var pos = 0; pos < order.length; pos++) {
        var idx = order[pos];
        var slot = makeSlot(pos, cardDistance, verticalDistance, total);
        placeNow(cards[idx], slot, skewAmount);
      }
    }

    function swapOnce() {
      if (order.length < 2) return;

      var front = order[0];
      var rest = order.slice(1);

      order = rest.concat(front);
      layoutAllNow();
    }

    /** 點擊切換：把被點到的卡移到最前，並重設自動輪播 */
    function goToCard(cardIndex) {
      var pos = order.indexOf(cardIndex);
      if (pos <= 0) return;

      order = order.slice(pos).concat(order.slice(0, pos));
      layoutAllNow();

      if (autoTimer) clearInterval(autoTimer);
      autoTimer = setInterval(swapOnce, intervalMs);
    }

    var autoTimer = setInterval(swapOnce, intervalMs);

    root.addEventListener('click', function (e) {
      var card = e.target.closest('.hn-card');
      if (!card) return;
      var idx = cards.indexOf(card);
      if (idx === -1) return;
      goToCard(idx);
    });

    layoutAllNow();
  });
})();
