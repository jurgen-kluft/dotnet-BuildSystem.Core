using System.IO;
using System.Collections.Generic;
using Core;

#region File Format

//      BuildTools: Just Another Dependency Checker, given a list of files like described
//      below in INPUT it will process these and output a file as described in OUTPUT.
//
//
// INPUT:
//
// FILE={
//     NAME=Models\Tracks\Track1\Track1.Track
//     IN={
//          NAME=Models\Tracks\Track1\Track1.mtl
//     }
//     IN={
//          NAME=Models\Tracks\Track1\Track1.obj
//     }
//     OUT={
//          NAME=Models\Tracks\Track1\Track1.bin
//     }
// }

// OUTPUT:
//
// FILE={
//     NAME=Models\Tracks\Track1\Track1.Track
//     HASH=a0d238cdsf750e3d845
//     IN={
//          NAME=Models\Tracks\Track1\Track1.mtl
//          HASH=a0d238cdsf750e3d845
//     }
//     IN={
//          NAME=Models\Tracks\Track1\Track1.obj
//     }
//     OUT={
//          NAME=Models\Tracks\Track1\Track1.bin
//          HASH=a0d238cdsf750e3d845
//     }
// }

#endregion
#region Remote Machines

/// We can extend the DependencySystem to hold multiple scopes, where the local 
/// machine is the local scope and other machines are remote scopes.
/// We can poll a remote scope to find out if the Dependency Info on that machine
/// gives a isModified(file)==false, if so we can copy those remote destination 
/// files to the local machine.

#endregion

namespace DataBuildSystem
{
    public class DependencySystem
    {
        #region Fields

        public enum EMachine
        {
            LOCAL,
            REMOTE,
        }

        private readonly DependencySystemImp mLocal;
        private readonly List<DependencySystemImp> mRemote = new List<DependencySystemImp>();

        #endregion
        #region Constructor

        public DependencySystem(Dirname srcPath, Dirname dstPath, Dirname dstDepPath)
        {
            mLocal = new DependencySystemImp(srcPath, dstPath, dstDepPath);
        }

        #endregion
        #region Properties

        public Dirname srcPath
        {
            get
            {
                return mLocal.srcPath;
            }
        }

        public Dirname dstPath
        {
            get
            {
                return mLocal.dstPath;
            }
        }

        public Dirname depPath
        {
            get
            {
                return mLocal.depPath;
            }
        }

        public bool anythingModified
        {
            get
            {
                return mLocal.anythingModified;
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// Add a dependency system from where we might copy destination files.
        /// </summary>
        /// <param name="inSrcPath"></param>
        /// <param name="inDstPath"></param>
        public void addMachine(Dirname inSrcPath, Dirname inDstPath, Dirname inDstDepPath)
        {
            if (Directory.Exists(inSrcPath) && Directory.Exists(inDstPath) && Directory.Exists(inDstDepPath))
                mRemote.Add(new DependencySystemImp(inSrcPath, inDstPath, inDstDepPath));
        }

        /// <summary>
        /// Load the dependency files
        /// </summary>
        /// <returns>True if load was successful</returns>
        public bool load()
        {
            if (!mLocal.build())
                return false;

            foreach (DependencySystemImp d in mRemote)
                if (!d.build())
                    return false;

            // Successful
            return true;
        }

        /// <summary>
        /// Load the dependency information of a src file
        /// </summary>
        /// <param name="masterDepfileFilename"></param>
        public bool load(DepFile depFile)
        {
            return depFile.load(depPath);
        }

        /// <summary>
        /// Load a dependency information file associated with a src file
        /// </summary>
        /// <param name="masterDepfileFilename"></param>
        internal DepFile[] loadDepFile(Filename srcfileFilename)
        {
            return DependencySystemImp.sLoadDepFile(depPath + srcfileFilename);
        }

        public void save(DepFile depFile)
        {
            depFile.save(depPath);
        }

        /// <summary>
        /// Save the dependency information into one file
        /// </summary>
        /// <param name="masterDepfileFilename"></param>
        public void loadCache(Filename masterDepfileFilename)
        {
            mLocal.loadCache(masterDepfileFilename);
        }

        /// <summary>
        /// Save the dependency information into one file
        /// </summary>
        /// <param name="masterDepfileFilename"></param>
        public void saveCache(Filename masterDepfileFilename)
        {
            mLocal.saveCache(masterDepfileFilename);
        }

        /// <summary>
        /// Update the status of all DepFiles
        /// </summary>
        public void update()
        {
            mLocal.update();

            foreach (DependencySystemImp d in mRemote)
                d.update();
        }

        /// <summary>
        /// Add a dependency
        /// </summary>
        /// <param name="inDepFile">A dependency file to add</param>
        public bool add(DepFile inDepFile)
        {
            return mLocal.add(inDepFile);
        }

        /// <summary>
        /// Add a dependency
        /// </summary>
        /// <param name="inDepFile">A dependency file to add</param>
        public DepFile get(Filename srcMainFilename)
        {
            return mLocal.get(srcMainFilename);
        }

        /// <summary>
        /// Register a dependency
        /// </summary>
        /// <param name="inDepFile">A dependency file to add, init and save</param>
        public bool register(DepFile inDepFile)
        {
            return mLocal.register(inDepFile);
        }

        /// <summary>
        /// Set dependency, add it or update it
        /// </summary>
        /// <param name="mainFilename">The main file</param>
        /// <param name="outputFilename">The generated output file</param>
        public bool set(Filename mainFilename, Filename outputFilename)
        {
            return mLocal.set(mainFilename, outputFilename);
        }

        /// <summary>
        /// Set dependency, add it or update it
        /// </summary>
        /// <param name="mainFilename">The main file</param>
        /// <param name="outputFilenames">The generated output files</param>
        public bool set(Filename mainFilename, List<Filename> outputFilenames)
        {
            return mLocal.set(mainFilename, outputFilenames);
        }
        /// <summary>
        /// Set dependency, add it or update it
        /// </summary>
        /// <param name="mainFilename">The main file</param>
        /// <param name="inputFilenames">The additional input files</param>
        /// <param name="outputFilenames">The generated output files</param>
        public bool set(Filename mainFilename, List<Filename> inputFilenames, List<Filename> outputFilenames)
        {
            return mLocal.set(mainFilename, inputFilenames, outputFilenames);
        }

        public bool remove(Filename mainFilename)
        {
            return mLocal.remove(mainFilename);
        }

        /// <summary>
        /// Determines the modification status of a DepFile
        /// </summary>
        /// <param name="mainFilename">The main file</param>
        /// <returns>Returns True if any of the dependencies of a DepFile is not equal to UNCHANGED.</returns>
        public bool isModified(Filename mainFilename)
        {
            return isModified(mainFilename, string.Empty);
        }

        public bool isModified(Filename mainFilename, string userKey)
        {
            return mLocal.isModified(mainFilename, userKey);
        }

        public bool isModified(Filename mainFilename, EMachine machine)
        {
            return (machine == EMachine.LOCAL) ? isModified(mainFilename) : isModified(mainFilename, string.Empty, machine);
        }

        public bool isModified(Filename mainFilename, string userKey, EMachine machine)
        {
            if (machine == EMachine.LOCAL)
                return isModified(mainFilename, userKey);

            if (mRemote.Count == 0)
                return true;

            return true;
        }

        #endregion
    }
}
