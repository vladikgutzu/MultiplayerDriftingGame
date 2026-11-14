# Main menu navigation — 2026-10-15

## Full animated-route regression

All 46 PanelAnimation source/destination routes passed active-state checks in
Play Mode using BD_MenuTransitionProbe. Returned to Main Menu with a live car
and home camera state. Raw log: Library/MenuAudit/transitions.txt.

MAP button and BACK were exercised separately: MinimapTarget/world-map canvas
on entry, CarTarget/Main Menu on return. Fixed near-plane restoration from 3.3
to 0.3 metres and cancel stale delayed map reveals on exit.

This checks panel state, not visual screenshot QA or real purchase/network flows.

Verified in the live Main Menu Play Mode using actual Button.onClick events:
SHOP opens Car Shop with active car buttons; BACK restores Main Menu.
DESIGN opens Car Tuning; COLOR opens Visual Tuning; WHEELS opens Wheel
Customization; DETAILS opens Tuning; Settings opens Options (Desktop).

Fixed transition coroutine lifetime: hiding the clicked button previously stopped
the coroutine before it could activate the destination. Main-menu transitions
now run on the persistent menu manager, with a realtime timeout for hidden-panel
waits. Null destinations no longer blank the UI. Nested destination ancestors
are activated, and OpenMenu preserves destination ancestors when closing siblings.

Restored serialized ChangeShopState callback used by shop buttons.

Not verified: every nested customization purchase, paid store flow, and network
room transition. Some legacy inactive buttons still contain missing targets;
those require individual feature validation rather than blind rewiring.
