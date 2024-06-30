/* eslint-disable max-len */
/* eslint-disable require-jsdoc */
const {onCall} = require("firebase-functions/v2/https");
const admin = require("firebase-admin");

// Initialize Firebase Admin
admin.initializeApp();

// Reference to Firebase Realtime Database
const database = admin.database();

exports.checkTopicAndWordTestt = onCall(async (request) => {
  const topicName = request.data.topicName;
  const word = request.data.word;

  console.log(`Topic name: ${topicName}, Word: ${word}`);

  try {
    // Check if the topic exists
    console.log(`Checking existence of topic '${topicName}'`);
    const topicSnapshot = await database.ref(`/topics/${topicName}`).once("value");

    if (!topicSnapshot.exists()) {
      console.log(`Topic '${topicName}' does not exist.`);
      throw new Error(`Topic '${topicName}' does not exist.`);
    }

    // Check if the word exists as primary or synonym under the topic
    console.log(`Checking existence of word '${word}' under topic '${topicName}'`);
    const wordExists = await checkWordExists(topicName, word);

    if (!wordExists) {
      console.log(`Word '${word}' does not exist under the topic '${topicName}'.`);
      throw new Error(`Word '${word}' does not exist under the topic '${topicName}'.`);
    }

    console.log(`Word '${word}' exists under the topic '${topicName}'.`);

    // Return a success message indicating that both topic and word exist
    return true;
  } catch (error) {
    console.error(`Error occurred: ${error.message}`);
    throw new Error(`Function execution error: ${error.message}`);
  }
});

async function checkWordExists(topicName, word) {
  const topicRef = database.ref(`/topics/${topicName}`);
  const snapshot = await topicRef.once("value");

  if (!snapshot.exists()) {
    return false;
  }

  let wordExists = false;
  snapshot.forEach((childSnapshot) => {
    const primaryWord = childSnapshot.child("primary").val();
    const synonyms = childSnapshot.child("synonyms").val();

    if (primaryWord === word || (synonyms && synonyms[word])) {
      wordExists = true;
    }
  });

  return wordExists;
}
