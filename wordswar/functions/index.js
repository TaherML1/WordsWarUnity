/* eslint-disable max-len */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

exports.deleteUserAccount = functions.https.onCall(async (data, context) => {
  const uid = context.auth.uid;

  if (!uid) {
    throw new functions.https.HttpsError("unauthenticated", "User must be authenticated");
  }

  // Delete user data from Realtime Database
  await admin.database().ref(`/users/${uid}`).remove();

  // Delete the user's authentication record
  await admin.auth().deleteUser(uid);

  // Delete user document from Firestore
  const userDocRef = admin.firestore().collection("users").doc(uid);
  await userDocRef.delete();

  return {message: "Account deleted successfully"};
});
