/* eslint-disable no-unused-vars */
/* eslint-disable max-len */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
const {logger} = require("./node_modules/firebase-functions/lib/v1/index");
admin.initializeApp();

exports.declineFriendRequest = functions.https.onCall(async (data, context) => {
  const userId = context.auth.uid;
  const requestId = data.requestId;

  if (!userId || !requestId) {
    throw new functions.https.HttpsError("invalid-argument", "The function must be called with valid arguments.");
  }

  try {
    const db = admin.firestore();

    // Fetch the friend request document to get the sender's ID
    const requestDoc = await db.collection("users").doc(userId).collection("friendRequests").doc(requestId).get();

    if (!requestDoc.exists) {
      throw new functions.https.HttpsError("not-found", "Friend request not found.");
    }

    const requestData = requestDoc.data();
    const senderId = requestData.senderId;
    const receiverId = requestData.receiverId;
    // const receiverAuthId = requestData.receiverAuthId;

    // Delete the friend request document from the receiver's friendRequests subcollection
    console.log("reciverId: " + receiverId);
    console.log("senderId : " + senderId);

    await db.collection("users").doc(userId).collection("friendRequests").doc(requestId).delete();

    // Delete the corresponding document from the sender's sentRequests subcollection
    await db.collection("users").doc(senderId).collection("sentRequests").doc(receiverId).delete();

    return {result: "Friend request declined successfully"};
  } catch (error) {
    console.error("Error declining friend request:", error);
    throw new functions.https.HttpsError("unknown", "Failed to decline friend request", error);
  }
});
