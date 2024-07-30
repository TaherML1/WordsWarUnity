/* eslint-disable no-undef */
/* eslint-disable max-len */
/* eslint-disable no-unused-vars */
// functions/index.js

const functions = require("firebase-functions");
const admin = require("firebase-admin");

admin.initializeApp(); // Initialize the Firebase Admin SDK
const firestore = admin.firestore();

exports.sendFriendRequest1 = functions.https.onCall(async (data, context) => {
  const senderId = data.senderId;
  const receiverPlayerId = data.receiverId;

  if (!senderId || !receiverPlayerId) {
    console.error("Invalid arguments:", {senderId, receiverPlayerId});
    throw new functions.https.HttpsError("invalid-argument", "The function must be called with two arguments \"senderId\" and \"receiverId\".");
  }

  try {
    const db = admin.firestore();

    // Fetch the receiver's document
    const receiverQuery = await db.collection("users").where("playerId", "==", receiverPlayerId).get();

    if (receiverQuery.empty) {
      console.error("Receiver not found for Player ID:", receiverPlayerId);
      throw new functions.https.HttpsError("not-found", "Receiver not found.");
    }

    const receiverDoc = receiverQuery.docs[0];
    const receiverRef = receiverDoc.ref;

    // Fetch the sender's document to get the username and playerId
    const senderDoc = await db.collection("users").doc(senderId).get();

    if (!senderDoc.exists) {
      console.error("Sender not found for ID:", senderId);
      throw new functions.https.HttpsError("not-found", "Sender not found.");
    }

    const senderData = senderDoc.data();
    const senderUsername = senderData.username || "Unknown"; // Use "Unknown" if the username field is missing
    const senderPlayerId = senderData.playerId || "Unknown";

    // Add the friend request to the receiver's friendRequests subcollection
    await receiverRef.collection("friendRequests").doc(senderId).set({
      senderId: senderId,
      username: senderUsername,
      playerId: senderPlayerId,
      timestamp: admin.firestore.FieldValue.serverTimestamp(),
    });

    console.log("Friend request sent successfully.");
    return {success: true};
  } catch (error) {
    console.error("Error sending friend request:", error);
    throw new functions.https.HttpsError("unknown", "An error occurred while sending the friend request.");
  }
});
