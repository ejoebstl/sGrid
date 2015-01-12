using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using System.Net;
using System.IO;
using System.Xml.Serialization;

namespace sGridServer.Code.GridProviders.WorldCommunityGrid
{
    /// <summary>
    /// This class implements the abstract GridProvider class for the use with WorldCommunityGrid. 
    /// 
    /// This class is a Mock-Up at the moment, since WCG did not neogiate with us by now. 
    /// </summary>
    [Obsolete("Obsolote API", true)]
    public class MockWorldCommunityGridProvider : GridProvider
    {
        //World Community Grid "Default" (Emi's) access data. 
        private const string Name = "emiswelt";
        private const string VerificationCode = "30c962295f6fbf620c3f58758206926d";
        private const string AccountKey = "b6c56e43daf4dd0760c41cb3cf51862a";
        private const string ProjectUrl = "http://www.worldcommunitygrid.org";
        private const string ProviderId = "WorldCommunityGrid_MockUp";

        //Statistic API url. Parameters: Username, VerificationCode
        private const string StatisticApiUrl = "http://www.worldcommunitygrid.org/verifyMember.do?name={0}&code={1}";

        /// <summary>
        /// Returns the description for WorldCommunityGrid.
        /// </summary>
        public static new GridProviderDescription Description
        {
            get
            {
                return new GridProviderDescription(ProviderId, "World Community Grid", "~/Content/images/gridProviders/wcg.jpg", 
                    "Our work has developed the technical infrastructure that serves as the grid's foundation for scientific research. Our success depends upon individuals collectively contributing their unused computer time to change the world for the better. World Community Grid is making technology available only to public and not-for-profit organizations to use in humanitarian research that might otherwise not be completed due to the high cost of the computer infrastructure required in the absence of a public grid. As part of our commitment to advancing human welfare, all results will be in the public domain and made public to the global research community.",
                    "World Community Grid's mission is to create the world's largest public computing grid to tackle projects that benefit humanity.", 
                    ProjectUrl, ProjectUrl,
                    new GridProjectDescription[]
                    {
                        new GridProjectDescription("gfam", 
                            "Malaria is one of the three deadliest infectious diseases on earth and is caused by parasites that infect both humans and animals. Female mosquitoes spread the disease by biting infected hosts and passing the parasites to other hosts that they bite later. When these parasites replicate themselves in red blood cells (which the parasites use for food), the symptoms of malaria appear. Malaria initially causes fevers and headaches, and in severe cases it leads to comas or death. Plasmodium falciparum, the parasite that causes the deadliest form of malaria, kills more people than any other parasite on the planet. Over 3 billion people are at risk of being infected with malaria. Although there are many approved drugs that are able to cure malarial infections, multi-drug-resistant mutant superbugs exist that are not eliminated by the current drugs. Because new mutant superbugs keep evolving and spreading throughout the world, discovering and developing new types of drugs that can cure infections by these multi-drug-resistant mutant strains of malaria is a significant global health priority.", 
                            "GO Fight Against Malaria",  "~/Content/images/gridProviders/globe.jpg", 
                            "The mission of the GO Fight Against Malaria project is to discover promising drug candidates that could be developed into new drugs that cure drug resistant forms of malaria. The computing power of World Community Grid will be used to perform computer simulations of the interactions between millions of chemical compounds and certain target proteins, to predict their ability to eliminate malaria. The best compounds will be tested and further developed into possible treatments for the disease.", 
                            ProjectUrl, ProjectUrl, 20, 700),
                        new GridProjectDescription("hcmd2", 
                            "Neuromuscular disease is a generic term for a group of disorders (more than 200 in all) that impair muscle functioning either directly through muscle pathology (muscular dystrophy) or indirectly through nerve pathology. Most of them are rare (affecting less than one person in 2,000), have a genetic origin (80%) and affect both children and adults. These chronic diseases lead to a decrease in muscle strength, causing serious disabilities in motor functions (moving, breathing etc.). Disease expression is variable; some disorders are progressive, while others remain stable for several years, and the same disease can cause different symptoms from one person to the next. Despite advances in therapeutic techniques, there is currently no curative treatment available for persons affected by neuromuscular diseases.", 
                            "Help Cure Muscular Dystrophy - Phase 2", "~/Content/images/gridProviders/globe.jpg", 
                            "World Community Grid and researchers supported by Decrypthon, a partnership between AFM (French Muscular Dystrophy Association), CNRS (French National Center for Scientific Research) and IBM are investigating protein-protein interactions for 40,000 proteins whose structures are known, with particular focus on those proteins that play a role in neuromuscular diseases. The database of information produced will help researchers design molecules to inhibit or enhance binding of particular macromolecules, hopefully leading to better treatments for muscular dystrophy and other neuromuscular diseases.", 
                            ProjectUrl, ProjectUrl, 20, 700),
                        new GridProjectDescription("hfcc", 
                            "Neuroblastoma is one of the most common tumors occuring in early childhood and is the most common cause of death in children with solid cancer tumors. If this project is successful, it could dramatically increase the cure rate for neuroblastoma, providing the breakthrough for this disease that has eluded scientists thus far. Proteins (molecules which are a bound collection of atoms) are the building blocks of all life processes. They also play an important role in the progress of diseases such as cancer. Scientists have identified three particular proteins involved with neuroblastoma, which if disabled, could make the disease much more curable by conventional methods such as chemotherapy. This project is performing virtual chemistry experiments between these proteins and each of the three million drug candidates that scientists believe could potentially block the proteins involved. A computer program called AutoDock will test if the shape of the protein and shape of each drug candidate fit together and bond in a suitable way to disable the protein. This work consists of 9 million virtual chemistry experiments, each of which would take hours to perform on a single computer, totaling over 8,000 years of computer time. World Community Grid is performing these computations in parallel and is thus speeding up the effort dramatically. The project is expected to be completed in two years or less. ", 
                            "Help Fight Childhood Cancer",  "~/Content/images/gridProviders/globe.jpg", 
                            "The mission of the Help Fight Childhood Cancer project is to find drugs that can disable three particular proteins associated with neuroblastoma, one of the most frequently occurring solid tumors in children. Identifying these drugs could potentially make the disease much more curable when combined with chemotherapy treatment.", 
                            ProjectUrl, ProjectUrl, 20, 700),
                        new GridProjectDescription("hcc1", 
                            "In order to significantly impact the understanding of cancer and its treatment, novel therapeutic approaches capable of targeting metastatic disease (or cancers spreading to other parts of the body) must not only be discovered, but also diagnostic markers (or indicators of the disease), which can detect early stage disease, must be identified. Researchers have been able to make important discoveries when studying multiple human cancers, even when they have limited or no information at all about the involved proteins. However, to better understand and treat cancer, it is important for scientists to discover novel proteins involved in cancer, and their structure and function. Scientists are especially interested in proteins that may have a functional relationship with cancer. These are proteins that are either over-expressed or repressed in cancers, or proteins that have been modified or mutated in ways that result in structural changes to them. Improving X-ray crystallography will enable researchers to determine the structure of many cancer-related proteins faster. This will lead to improving our understanding of the function of these proteins and enable potential pharmaceutical interventions to treat this deadly disease.", 
                            "Help Conquer Cancer",  "~/Content/images/gridProviders/globe.jpg", 
                            "The mission of Help Conquer Cancer is to improve the results of protein X-ray crystallography, which helps researchers not only annotate unknown parts of the human proteome, but importantly improves their understanding of cancer initiation, progression and treatment.", 
                            ProjectUrl, ProjectUrl, 20, 700),
                        new GridProjectDescription("hpf2", 
                            "The project, which began at the Institute for Systems Biology and now continues at New York University's Department of Biology and Computer Science, will refine, using the Rosetta software in a mode that accounts for greater atomic detail, the structures resulting from the first phase of the project. The goal of the first phase was to understand protein function. The goal of the second phase is to increase the resolution of the predictions for a select subset of human proteins. Better resolution is important for a number of applications, including but not limited to virtual screening of drug targets with docking procedures and protein design. By running a handful of well-studied proteins on World Community Grid (like proteins from yeast), the second phase also will serve to improve the understanding of the physics of protein structure and advance the state-of-the-art in protein structure prediction. This also will help the Rosetta developers community to further develop the software and the reliability of its predictions. HPF2 will focus on human-secreted proteins (proteins in the blood and the spaces between cells). These proteins can be important for signaling between cells and are often key markers for diagnosis. These proteins have even ended up being useful as drugs (when synthesized and given by doctors to people lacking the proteins). Examples of human secreted proteins turned into therapeutics are insulin and the human growth hormone. Understanding the function of human secreted proteins may help researchers discover the function of proteins of unknown function in the blood and other interstitial fluids. The project also will focus on key secreted pathogenic proteins. While still in its early design phases, HPF2 will likely focus on Plasmodium, the pathogenic agent that causes malaria. Researchers hope that higher resolution structure predictions for the proteins that malaria secretes will serve as bioinformatics infrastructure for researchers who are working hard around the world to understand the complex interaction between human hosts and malaria parasites. While there are few silver bullets, and biology is one of the most complicated subjects on earth, researchers believe that this work will help it understand elements of this host-pathogen interaction or at least its components. Researchers will provide their findings as a resource to the scientific community and then work with the community on visualizing, using and refining the data. This understanding could then be a foundation for intervention. Lastly, this project dovetails with efforts at NYU and ISB to support predictive, preventative and personalized medicine (under the assumption that these secreted proteins will be key elements of this medicine of the future). It is too early to say which proteins will end up being biomarkers (substances sometimes found in an increased amount in the blood, other body fluids, or tissues and which can be used to indicate the presence of some types of cancer). However, it is clear that many will end up being secreted proteins. As in the first phase of the project, the power of World Community Grid will be critical in getting results quickly to researchers in the biological and biomedical communities.", 
                            "Human Proteome Folding - Phase 2",  "~/Content/images/gridProviders/globe.jpg", 
                            "Human Proteome Folding Phase 2 (HPF2) continues where the first phase left off. The two main objectives of the project are to: 1) obtain higher resolution structures for specific human proteins and pathogen proteins and 2) further explore the limits of protein structure prediction by further developing Rosetta software structure prediction. Thus, the project will address two very important parallel imperatives, one biological and one biophysical.", 
                            ProjectUrl, ProjectUrl, 20, 700),
                        new GridProjectDescription("c4cw", 
                            "Lack of access to clean water is one of the major humanitarian challenges for many regions in the developing world. It is estimated that 1.2 billion people lack access to safe drinking water, and 2.6 billion have little or no sanitation. Millions of people die annually - estimates are 3,900 children a day - from the results of diseases transmitted through unsafe water, in particular diarrhea. Technologies for filtering dirty water exist, but are generally quite expensive. Desalination of sea water, a potentially abundant source of drinking water, is similarly limited by filtering costs. Therefore, new approaches to efficient water filtering are a subject of intense research. Carbon nanotubes, stacked in arrays so that water must pass through the length of the tubes, represent a new approach to filtering water. Normally, the extremely small pore size of nanotubes, typically only a few water molecules in diameter, would require very large pressures and hence expensive equipment in order to filter useful amounts of water. However, in 2005 experiments showed that such arrays of nanotubes allow water to flow at much higher rates than expected. This surprising result has spurred many scientists to invest considerable effort in studying the underlying processes that facilitate water flow in nanotubes. This project uses large-scale molecular dynamics calculations - where the motions of individual water molecules through the nanotubes are simulated - in order to get a deeper understanding of the mechanism of water flow in the nanotubes. For example, there has been speculation about whether the water molecules in direct contact with the nanotube might behave more like ice. This in turn might reduce the friction felt by the rest of the water, hence increasing the rate of flow. Realistic computer simulations are one way to test such hypotheses. Ultimately, the scientists hope to use the insights they glean from the simulations in order to optimize the underlying process that is enabling water to flow much more rapidly through nanotubes and other nanoporous materials. This optimization process will allow water to flow even more easily, while retaining sources of contamination. The simulations may also reveal under what conditions such filters can best assist in a desalination process.", 
                            "Computing for Clean Water",  "~/Content/images/gridProviders/globe.jpg", 
                            "The mission of Computing for Clean Water is to provide deeper insight on the molecular scale into the origins of the efficient flow of water through a novel class of filter materials. This insight will in turn guide future development of low-cost and more efficient water filters.", 
                            ProjectUrl, ProjectUrl, 20, 700)
                    },
                    typeof(MockWorldCommunityGridProvider));
            }
        }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        public MockWorldCommunityGridProvider() : base(Description)
        {

        }

        /// <inheritdoc />
        protected override GridProviderAuthenticationData RegisterUserWithProjectServer(DataAccessLayer.Models.User u)
        {
            //Return "Default" (Emi's) user token to enable debugging. 
            GridProviderAuthenticationData data = new GridProviderAuthenticationData()
            {
                AuthenticationInfo = AccountKey,
                ProviderId = ProviderId,
                UserId = u.Id,
                UserName = Name,
                UserToken = VerificationCode
            };

            //TODO: Register the user with WCG - Now skipped because mock up. 

            return data;
        }

        /// <inheritdoc />
        protected override void RemoveUserFromProjectServer(User u, GridProviderAuthenticationData data)
        {
            //TODO: Un-Register User with grid provider - Now skipped because mock up. 
        }

        //No longer used.

        //protected GridPerformanceData GetStatisticsFromProjectServer(User u)
        //{
        //    //Gather the performance data from world community grid using Xml-De-Serialization
        //    string url = String.Format(StatisticApiUrl, Name, VerificationCode);

        //    WebClient client = new WebClient();
        //    Stream stream = client.OpenRead(url);
        //    XmlSerializer serializer = new XmlSerializer(typeof(MemberStatsWithTeamHistory));

        //    MemberStatsWithTeamHistory wcgData = serializer.Deserialize(stream) as MemberStatsWithTeamHistory;

        //    stream.Close();
        //    client.Dispose();

        //    //Now, analyze the data.
        //    List<ProjectPerformanceData> projectDataList = new List<ProjectPerformanceData>();

        //    MemberStatsWithTeamHistoryMemberStatsMemberStat memberStatistic = wcgData.MemberStats[0].MemberStat[0];
        //    MemberStatsWithTeamHistoryMemberStatsMemberStatStatisticsTotals memberStatisticTotals = memberStatistic.StatisticsTotals[0];

        //    //Foreach project which was reported by WCG...
        //    foreach (MemberStatsWithTeamHistoryMemberStatsByProjectsProject projectStatistic in wcgData.MemberStatsByProjects[0].Project)
        //    {
        //        //...create a project snapshot...
        //        GridProjectDescription projectDescription = GridProviderManager.ProjectForName(projectStatistic.ProjectShortName);

        //        ProjectPerformanceData newSnapshot = new ProjectPerformanceData()
        //        {
        //            ProjectShortName = projectStatistic.ProjectShortName,
        //            ProviderId = ProviderId,
        //            ResultCount = projectStatistic.Results,
        //            Runtime = projectStatistic.RunTime,
        //            Timestamp = DateTime.Now,
        //            UserId = u.Id
        //        };

        //        //... and compare whether the result count increased.
        //        ProjectPerformanceData currentSnapshot = CurrentPerformance.Where(x => x.UserId == u.Id && x.ProjectShortName == projectDescription.ShortName).FirstOrDefault();

        //        if (currentSnapshot != null)
        //        {
        //            //If there is a result count difference, mark some results as valid. 
        //            int resultDiff = projectStatistic.Results - currentSnapshot.ResultCount;

        //            for (int i = 0; i < resultDiff; i++)
        //            {
        //                if (!MarkResultAsValid(u, projectDescription, TimeSpan.Zero))
        //                {
        //                    break;
        //                }
        //            }
        //        }

        //        projectDataList.Add(newSnapshot);
        //    }

        //    //Finally, construct a summary. 
        //    GridPerformanceData gridData = new GridPerformanceData(memberStatistic.NumDevices,
        //        base.Description,
        //        projectDataList,
        //        memberStatisticTotals.Results,
        //        memberStatistic.LastResult,
        //        memberStatisticTotals.RunTime,
        //        u);
        //    return gridData;

        //}
        
    }
}