/* eslint-disable max-len */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

exports.deleteFriend = functions.https.onCall(async (data, context) => {
  const userId = context.auth.uid;
  const friendId = data.friendId;

  if (!userId) {
    throw new functions.https.HttpsError("unauthenticated", "User is not authenticated.");
  }

  if (!friendId) {
    throw new functions.https.HttpsError("invalid-argument", "Friend ID is required.");
  }

  const userRef = admin.firestore().collection("users").doc(userId).collection("friends").doc(friendId);
  const friendRef = admin.firestore().collection("users").doc(friendId).collection("friends").doc(userId);

  try {
    await userRef.delete();
    await friendRef.delete();
    return {success: true};
  } catch (error) {
    console.error("Error deleting friend:", error);
    throw new functions.https.HttpsError("internal", "Error deleting friend.");
  }
});
