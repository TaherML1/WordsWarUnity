/* eslint-disable max-len */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

exports.declineFriendRequest = functions.https.onCall((data, context) => {
  const userId = context.auth.uid;
  const requestId = data.requestId;

  if (!userId || !requestId) {
    throw new functions.https.HttpsError("invalid-argument", "The function must be called with valid arguments.");
  }

  return admin.firestore().collection("users").doc(userId).collection("friendRequests").doc(requestId).delete()
      .then(() => {
        return {result: "Friend request declined successfully"};
      })
      .catch((error) => {
        throw new functions.https.HttpsError("unknown", "Failed to decline friend request", error);
      });
});
