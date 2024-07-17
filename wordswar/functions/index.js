/* eslint-disable max-len */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

exports.updateServerTime = functions.pubsub.schedule("every 7 minutes").onRun((context) => {
  const ref = admin.firestore().collection("serverTime").doc("currentTime");
  return ref.set({time: admin.firestore.FieldValue.serverTimestamp()});
});
