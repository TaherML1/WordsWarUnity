/* eslint-disable max-len */
const functions = require("firebase-functions");
const admin = require("firebase-admin");

admin.initializeApp();

// Reference to Firebase Realtime Database
const database = admin.database();

exports.checkWordUsage = functions.https.onCall(async (data, context) => {
  const roomId = data.roomId;
  const topicName = data.topicName;
  const word = data.word;

  console.log(`Room ID: ${roomId}, Topic: ${topicName}, Word: ${word}`);

  try {
    // Fetch the topic details to find primary and synonyms
    const topicRef = database.ref(`/topics/${topicName}`);
    const topicSnapshot = await topicRef.once("value");

    if (!topicSnapshot.exists()) {
      console.log(`Topic '${topicName}' does not exist.`);
      throw new functions.https.HttpsError("not-found", `Topic '${topicName}' does not exist.`);
    }

    let primaryWord = "";
    let synonyms = {};

    topicSnapshot.forEach((wordSnapshot) => {
      const primary = wordSnapshot.child("primary").val();
      const syns = wordSnapshot.child("synonyms").val();

      if (primary === word || (syns && Object.keys(syns).includes(word))) {
        primaryWord = primary;
        synonyms = syns;
      }
    });

    if (!primaryWord) {
      console.log(`Word '${word}' not found under topic '${topicName}'.`);
      throw new functions.https.HttpsError("not-found", `Word '${word}' not found under topic '${topicName}'.`);
    }

    // Check if the primary word or any of its synonyms have been used
    const usedWordsRef = database.ref(`/games/${roomId}/gameInfo/usedwords`);
    const usedWordsSnapshot = await usedWordsRef.once("value");

    if (usedWordsSnapshot.exists()) {
      let wordUsed = false;

      usedWordsSnapshot.forEach((playerSnapshot) => {
        playerSnapshot.forEach((usedWordSnapshot) => {
          const usedWord = usedWordSnapshot.val().toLowerCase();

          if (usedWord === primaryWord.toLowerCase() || (synonyms && synonyms[usedWord])) {
            wordUsed = true;
          }
        });
      });

      if (wordUsed) {
        console.log(`The word or its synonyms have already been used.`);
        return true;
      }
    }

    console.log(`The word and its synonyms have not been used.`);
    return false;
  } catch (error) {
    console.error("Error checking word usage:", error.message);
    throw new functions.https.HttpsError("internal", error.message);
  }
});
