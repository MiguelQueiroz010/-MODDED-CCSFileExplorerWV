# CCS File Explorer - Version 1.0.1 #


### What is this repository for? ###

This is a repo for the mod of CCSFE used for the Eden Server and by extension, The World V:R. 
It allows for easy exporting of .hack models and textures. It was originally developed by WV but has been significantly enhanced since then.

### How do I get set up? ###

To extract CCS Files from the original games, you'll have to follow one of two methods:

IMOQF:
Download one of the R1-series .hack games and open up the iso file in any unarchiving application. 
Extract the DATA.BIN file and open CCSFE. 
Once you have CCSFE open, navigate to file -> Unpack BIN and select the DATA.BIN. It should provide you with a folder full of CCS files from the IMOQF titles. From there you can open and navigate through the avaialable MDL models and bmp textures.

GU:
Download .hack GU Volume 3 and open the iso file up in 7-zip (specifically 7zip on this one).
Extract the file labeled DATA.CVM and open it once again in 7-zip.
Once you do, you'll see a set of folders and CCS files. However, these CCS files actually have to be unpacked an additional time. Extract the file you'd like to open from the DATA.CVM and rename it from .CCS to .BIN, then use CCS File Explorer to unpack the bin like described above. It should provide you with a new CCS file that is able to be opened inside of the file explorer.

### Known Bugs/Limitations ###

-Any models with rigging (Characters) will not be read or export currently.
-Currently, only textures and non-rigged models are exportable, so lighting, particles, and animations are also not supported.
-A very, very small percentage of area models (such as the Gears in Breg Epona and all normal Field models save from the skybox in GU) are not read correctly and as a result come out as a complete mess.
-Some specific area models don't read at all, this issue is also being looked into. May have to do with the rigging issue above as it seems to be very very selective and happens rarely.
-Textures currently need to be applied once the model has been exported from CCSFE
-Currently, the repacking feature doesn't repack the IMOQ Data.BIN in a way that it is able to be inserted back into a .hack game.

