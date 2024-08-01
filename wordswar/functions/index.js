/* eslint-disable max-len */
/* eslint-disable no-unused-vars */
// functions/index.js

const {onCall} = require("firebase-functions/v2/https");
const {initializeApp} = require("firebase-admin/app");
const {getFirestore, FieldValue} = require("firebase-admin/firestore");

initializeApp();
const firestore = getFirestore();

exports.acceptFriendRequest = onCall(async (request) => {
  const {senderId, receiverId} = request.data;

  try {
    const senderRef = firestore.collection("users").doc(senderId);
    const receiverRef = firestore.collection("users").doc(receiverId);

    // Fetch usernames, playerIds, scores, and levels
    const senderDoc = await senderRef.get();
    const receiverDoc = await receiverRef.get();

    if (!senderDoc.exists || !receiverDoc.exists) {
      throw new Error("Sender or receiver does not exist.");
    }

    const senderData = senderDoc.data();
    const receiverData = receiverDoc.data();

    const senderUsername = senderData.username || "Unknown";
    const senderPlayerId = senderData.playerId || "Unknown";


    const receiverUsername = receiverData.username || "Unknown";
    const receiverPlayerId = receiverData.playerId || "Unknown";


    // Add each other as friends
    await senderRef.collection("friends").doc(receiverId).set({
      friendId: receiverId,
      username: receiverUsername,
      playerId: receiverPlayerId,

      timestamp: FieldValue.serverTimestamp(),
    });

    await receiverRef.collection("friends").doc(senderId).set({
      friendId: senderId,
      username: senderUsername,
      playerId: senderPlayerId,

      timestamp: FieldValue.serverTimestamp(),
    });

    // Remove the friend request from the receiver's friendRequests subcollection
    await receiverRef.collection("friendRequests").doc(senderId).delete();

    return {success: true, message: "Friend request accepted successfully."};
  } catch (error) {
    return {success: false, message: error.message};
  }
});
