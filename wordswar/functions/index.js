/* eslint-disable no-undef */
exports.getServerTime = functions.https.onCall((data, context) => {
  console.log("getServerTime called");
  const serverTime = admin.firestore.Timestamp.now();
  return {dateTime: serverTime.toDate().toISOString()};
});
